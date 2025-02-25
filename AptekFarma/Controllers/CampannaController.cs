﻿using AptekFarma.Models;
using AptekFarma.DTO;
using AptekFarma.Context;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Humanizer;


namespace AptekFarma.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CampannaController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CampannaController(
            AppDbContext context)
        {
            _context = context;
        }

        [HttpPost("GetAllCampannas")]
        [RequestSizeLimit(70 * 1024 * 1024)]
        public async Task<IActionResult> GetAllCampannas()
        {
            return Ok(await _context.Campanna
                .Include(c => c.EstadoCampanna)
                .Include(c => c.VideoArchivo)
                .Where(c => c.Activo == true).OrderByDescending(c => c.FechaInicio).ToListAsync());
        }

        [HttpGet("GetCampannaById")]
        public async Task<IActionResult> GetCampannaById(int id)
        {
            var campanna = await _context.Campanna.Include(c => c.EstadoCampanna)
                .Include(c => c.VideoArchivo)
                .FirstOrDefaultAsync(x => x.Id == id);
            var productos = new List<ProductoCampanna>();
            if (campanna != null)
            {
                productos = await _context.ProductoCampanna.Where(x => x.CampannaId == campanna.Id && x.Activo == true).OrderByDescending(x=> x.Id).ToListAsync();
            }

            if (campanna == null)
            {
                return NotFound(new { message = "No se ha encontrado Campaña" });
            }

            return Ok(new { campanna, productos });

        }


        [HttpGet("GetCampannaByIdRankings")]
        public async Task<IActionResult> GetCampannaById(int id, [FromQuery] string userId)
        {
            var campanna = await _context.Campanna
                .Include(c => c.EstadoCampanna)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (campanna == null)
            {
                return NotFound(new { message = "No se ha encontrado Campaña" });
            }

            var productos = await _context.ProductoCampanna
                .Where(x => x.CampannaId == campanna.Id && x.Activo == true).OrderByDescending(x => x.Id)
                .ToListAsync();

            var formularios = await _context.FormularioVenta
                .Where(f => f.CampannaID == id)
                .Include(f => f.User).ThenInclude(u => u.Pharmacy)
                .ToListAsync();

            var rankingFarmacias = formularios
                .GroupBy(f => f.User.PharmacyID)
                .Select(g => new
                {
                    PharmacyId = g.Key,
                    PharmacyName = g.FirstOrDefault()?.User.Pharmacy?.Nombre ?? "Sin nombre",
                    TotalVentas = g.Sum(f => f.TotalPuntos)
                })
                .OrderByDescending(x => x.TotalVentas)
                .Take(3)
                .ToList();

            var farmaciaIdUsuario = await _context.Users
                .Where(u => u.Id == userId)
                .Select(u => u.PharmacyID)
                .FirstOrDefaultAsync();

            var rankingUsuariosFarmacia = formularios
                .Where(f => f.User.PharmacyID == farmaciaIdUsuario)
                .GroupBy(f => f.UserID)
                .Select(g => new
                {
                    UserId = g.Key,
                    UserName = g.FirstOrDefault()?.User?.nombre ?? "Sin nombre",
                    TotalVentas = g.Sum(f => f.TotalPuntos)
                })
                .OrderByDescending(x => x.TotalVentas)
                .Take(3)
                .ToList();

            var formulariosUsuario = await _context.FormularioVenta
                .Where(f => f.CampannaID == id && f.UserID == userId)
                .ToListAsync();


            var ventaProductosUsuario = await (from v in _context.VentaCampanna
                                               join f in _context.FormularioVenta
                                               on v.FormularioID equals f.Id
                                               where f.UserID == userId && f.CampannaID == id
                                               select new
                                               {
                                                   ProductoId = v.ProductoCampanna.Id,
                                                   ProductoNombre = v.ProductoCampanna.Nombre,
                                                   CantidadVendida = v.Cantidad
                                               })
                       .GroupBy(v => v.ProductoId)
                       .Select(g => new
                       {
                           ProductoId = g.Key,
                           ProductoNombre = g.FirstOrDefault().ProductoNombre,
                           CantidadVendida = g.Sum(v => v.CantidadVendida)
                       })
                       .OrderByDescending(x => x.CantidadVendida)
                       .Take(3)
                       .ToListAsync();


            return Ok(new
            {
                campanna,
                productos,
                rankings = new
                {
                    topFarmacias = rankingFarmacias,
                    topUsuariosFarmacia = rankingUsuariosFarmacia,
                    topProductosUsuario = ventaProductosUsuario
                }
            });
        }


        [HttpPost("CreateCampanna")]
        [RequestSizeLimit(70 * 1024 * 1024)]
        public async Task<IActionResult> CreateCampanna([FromForm] CrearCampannaDTO campannaDTO)
        {

            var campanna = new Campanna
            {
                Nombre = campannaDTO.nombre,
                Titulo = campannaDTO.titulo,
                Descripcion = campannaDTO.descripcion,
                Importante = campannaDTO.importante,
                Imagen = campannaDTO.imagen,
                FechaInicio = campannaDTO.fechaInicio,
                FechaFin = campannaDTO.fechaFin,
                FechaValido = campannaDTO.fechaValido,
                Activo = true
            };

            if (campannaDTO.video != null)
            {
                campanna.Video = campannaDTO.video;
            }

            // Asignar el estado de la campaña dependiendo de si la fecha actual está entre la fecha de inicio y fin
            if (DateTime.Now.Date >= campanna.FechaInicio.Date && DateTime.Now.Date <= campanna.FechaFin.Date)
            {
                campanna.EstadoCampanna = await _context.EstadoCampanna.FirstOrDefaultAsync(x => x.Id == 1);
            }
            else
            {
                campanna.EstadoCampanna = await _context.EstadoCampanna.FirstOrDefaultAsync(x => x.Id == 2);
            }

            await _context.Campanna.AddAsync(campanna);
            await _context.SaveChangesAsync();

            if (campannaDTO.pdf != null)
            {

                // Crear la carpeta si no existe
                var folderPath = Path.Combine("wwwroot", "campannas", "pdfs", campanna.Id.ToString());
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                var imagePath = Path.Combine(folderPath, campannaDTO.pdf.FileName);

                // Guardar la imagen en la carpeta estática
                using (var stream = new FileStream(imagePath, FileMode.Create))
                {
                    await campannaDTO.pdf.CopyToAsync(stream);
                }

                // Guardar la URL relativa en la base de datos
                var relativeImagePath = Path.Combine("campannas", "pdfs", campanna.Id.ToString(), campannaDTO.pdf.FileName);
                // Reeemplazar las barras invertidas por barras normales
                relativeImagePath = relativeImagePath.Replace("\\", "/");

                campanna.PDF = relativeImagePath;
                _context.Campanna.Update(campanna);
                await _context.SaveChangesAsync();
            }

            if (campannaDTO.videoArchivo != null)
            {

                var videoFolderPath = Path.Combine("wwwroot", "campannas", "videos", campanna.Id.ToString());
                if (!Directory.Exists(videoFolderPath))
                    Directory.CreateDirectory(videoFolderPath);

                var videoPath = Path.Combine(videoFolderPath, campannaDTO.videoArchivo.FileName);
                using (var stream = new FileStream(videoPath, FileMode.Create))
                {
                    await campannaDTO.videoArchivo.CopyToAsync(stream);
                }

                var nuevoVideo = new Video
                {
                    Nombre = campannaDTO.videoArchivo.FileName,
                    Ruta = Path.Combine("campannas", "videos", campanna.Id.ToString(), campannaDTO.videoArchivo.FileName).Replace("\\", "/"),
                    FechaSubida = DateTime.UtcNow
                };

                _context.Video.Add(nuevoVideo);
                await _context.SaveChangesAsync();

                campanna.VideoId = nuevoVideo.Id;
                _context.Campanna.Update(campanna);
                await _context.SaveChangesAsync();
            }


            var campannas = await _context.Campanna.Include(c => c.EstadoCampanna)
                .Where(c => c.Activo == true)
                .ToListAsync();

            return Ok(new { message = "Campaña creada correctamente", campannas });
        }

        [HttpPut("UpdateCampanna")]
        [RequestSizeLimit(70 * 1024 * 1024)]
        public async Task<IActionResult> UpdateCampanna([FromForm] UpdateCampannaDTO campannaDTO)
        {
            var campanna = await _context.Campanna
                .Include(c => c.VideoArchivo)
                .FirstOrDefaultAsync(x => x.Id == campannaDTO.id);


            if (campanna == null)
            {
                return NotFound(new { message = "No se ha encontrado Campaña" });
            }

            campanna.Nombre = campannaDTO.nombre;
            campanna.Titulo = campannaDTO.titulo;
            campanna.Importante = campannaDTO.importante;
            campanna.Descripcion = campannaDTO.descripcion;
            campanna.FechaInicio = campannaDTO.fechaInicio;
            campanna.FechaFin = campannaDTO.fechaFin;
            campanna.FechaValido = campannaDTO.fechaValido;

            if(campannaDTO.imagen != null)
            {
                campanna.Imagen = campannaDTO.imagen;
            }

            if (campannaDTO.video != null)
            {
                campanna.Video = campannaDTO.video;
            }

            // Asignar el estado de la campaña dependiendo de si la fecha actual está entre la fecha de inicio y fin
            if (DateTime.Now.Date >= campanna.FechaInicio.Date && DateTime.Now.Date <= campanna.FechaFin.Date)
            {
                campanna.EstadoCampanna = await _context.EstadoCampanna.FirstOrDefaultAsync(x => x.Id == 1);
            }
            else
            {
                campanna.EstadoCampanna = await _context.EstadoCampanna.FirstOrDefaultAsync(x => x.Id == 2);
            }


            _context.Campanna.Update(campanna);
            await _context.SaveChangesAsync();

            if (campannaDTO.pdf != null)
            {

                // Crear la carpeta si no existe
                var folderPath = Path.Combine("wwwroot", "campannas", "pdfs", campanna.Id.ToString());
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                var imagePath = Path.Combine(folderPath, campannaDTO.pdf.FileName);

                // Guardar la imagen en la carpeta estática
                using (var stream = new FileStream(imagePath, FileMode.Create))
                {
                    await campannaDTO.pdf.CopyToAsync(stream);
                }

                // Guardar la URL relativa en la base de datos
                var relativeImagePath = Path.Combine("campannas", "pdfs", campanna.Id.ToString(), campannaDTO.pdf.FileName);
                // Reeemplazar las barras invertidas por barras normales
                relativeImagePath = relativeImagePath.Replace("\\", "/");

                campanna.PDF = relativeImagePath;
                _context.Campanna.Update(campanna);
                await _context.SaveChangesAsync();
            }

            if (campannaDTO.videoArchivo != null)
            {

                if (campanna.VideoArchivo != null)
                {
                    var videoAnteriorPath = Path.Combine("wwwroot", campanna.VideoArchivo.Ruta);
                    if (System.IO.File.Exists(videoAnteriorPath))
                    {
                        System.IO.File.Delete(videoAnteriorPath);
                    }

                    _context.Video.Remove(campanna.VideoArchivo);
                    await _context.SaveChangesAsync();
                }

                var videoFolderPath = Path.Combine("wwwroot", "campannas", "videos", campanna.Id.ToString());
                if (!Directory.Exists(videoFolderPath))
                    Directory.CreateDirectory(videoFolderPath);

                var videoPath = Path.Combine(videoFolderPath, campannaDTO.videoArchivo.FileName);
                using (var stream = new FileStream(videoPath, FileMode.Create))
                {
                    await campannaDTO.videoArchivo.CopyToAsync(stream);
                }


                var nuevoVideo = new Video
                {
                    Nombre = campannaDTO.videoArchivo.FileName,
                    Ruta = Path.Combine("campannas", "videos", campanna.Id.ToString(), campannaDTO.videoArchivo.FileName).Replace("\\", "/"),
                    FechaSubida = DateTime.UtcNow
                };

                _context.Video.Add(nuevoVideo);
                await _context.SaveChangesAsync();


                campanna.VideoId = nuevoVideo.Id;
                _context.Campanna.Update(campanna);
                await _context.SaveChangesAsync();
            }
            else {

                if (campanna.VideoArchivo != null)
                {
                    var videoAnteriorPath = Path.Combine("wwwroot", campanna.VideoArchivo.Ruta);
                    if (System.IO.File.Exists(videoAnteriorPath))
                    {
                        System.IO.File.Delete(videoAnteriorPath);
                    }

                    _context.Video.Remove(campanna.VideoArchivo);
                    await _context.SaveChangesAsync();
                }

                campanna.VideoId = null;
                _context.Campanna.Update(campanna);
            }
                var campannas = await _context.Campanna
                    .Where(c => c.Activo == true)
                    .Include(c => c.EstadoCampanna)
                    .Include(c => c.VideoArchivo)
                    .ToListAsync();

            return Ok(new { message = "Campaña editada correctamente", campannas });
        }

        [HttpDelete("DeleteCampanna")]
        public async Task<IActionResult> DeleteCampanna(int id)
        {
            var campanna = await _context.Campanna
                .Include(c => c.EstadoCampanna)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (campanna == null)
            {
                return NotFound(new { message = "No se ha encontrado Campaña" });
            }

            campanna.Activo = false;
            await _context.SaveChangesAsync();
            var campannas = await _context.Campanna.Include(c => c.EstadoCampanna)
                .Where(c => c.Activo == true)
                .ToListAsync();
            return Ok(new { message = "Eliminada Correctamente", campannas });
        }
        [HttpGet("GetCampannaInformes")]
        public async Task<IActionResult> GetCampanna([FromQuery] string userID)
        {
            if (string.IsNullOrEmpty(userID))
            {
                return BadRequest("Debe proporcionar un UserID.");
            }

            var user = await _context.Users.FindAsync(userID);
            if (user == null)
            {
                return NotFound("Usuario no encontrado.");
            }

            var campannas = await _context.Campanna
                .Include(c => c.EstadoCampanna)
                .Include(c => c.VideoArchivo)
                .Where(c => c.Activo == true)
                .ToListAsync();

            var campannaDTOs = new List<CampannaDTO>();
            if (campannas == null || !campannas.Any())
            {
                return Ok(campannaDTOs);
            }

            foreach (var campanna in campannas)
            {
                var formularios = await _context.FormularioVenta
                    .Where(f => f.CampannaID == campanna.Id && f.UserID == userID)
                    .ToListAsync();

                var formulariosNoValidados = formularios
                    .Where(f => f.EstadoFormularioID == 1)
                    .ToList();

                var formulariosValidados = formularios
                    .Where(f => f.EstadoFormularioID == 2)
                    .ToList();

                var campannaDTO = new CampannaDTO
                {
                    id = campanna.Id,
                    nombre = campanna.Nombre,
                    titulo = campanna.Titulo,
                    imagen = campanna.Imagen,
                    importante = campanna.Importante,
                    descripcion = campanna.Descripcion,
                    fechaInicio = campanna.FechaInicio,
                    fechaFin = campanna.FechaFin,
                    fechaValido = campanna.FechaValido,
                    estadoCampanna = campanna.EstadoCampanna,
                    PDF = campanna.PDF,
                    Video = campanna.Video,
                    informesPendientes = formulariosNoValidados.Count,
                    informesConfirmados = formulariosValidados.Count,
                    puntosObtenidos = formulariosValidados.Sum(f => f.TotalPuntos),
                    videoArchivo = campanna.VideoArchivo
                };

                campannaDTOs.Add(campannaDTO);
            }

            campannaDTOs = campannaDTOs.OrderByDescending(c => c.fechaInicio).ToList();
            return Ok(campannaDTOs);
        }

        [HttpGet("GetPdfByCampannaId")]
        public async Task<IActionResult> GetPdfByCampannaId(int id)
        {
            var campanna = await _context.Campanna
                .FirstOrDefaultAsync(c => c.Id == id);

            if (campanna == null)
            {
                return NotFound(new { message = "No se ha encontrado la campaña" });
            }

            if (string.IsNullOrEmpty(campanna.PDF))
            {
                return NotFound(new { message = "No se ha encontrado el archivo PDF asociado a esta campaña" });
            }

            var pdfFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", campanna.PDF);

            if (!System.IO.File.Exists(pdfFilePath))
            {
                return NotFound(new { message = "El archivo PDF no se encuentra en el servidor" });
            }

            var fileBytes = await System.IO.File.ReadAllBytesAsync(pdfFilePath);

            return File(fileBytes, "application/pdf", campanna.PDF);
        }

        [HttpGet("GetVideoByCampannaId")]
        [RequestSizeLimit(70 * 1024 * 1024)]
        public async Task<IActionResult> GetVideoByCampannaId(int id)
        {
            var campanna = await _context.Campanna
                .Include(c => c.VideoArchivo)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (campanna == null)
                return NotFound(new { message = "No se ha encontrado la campaña" });

            if (campanna.VideoArchivo == null || string.IsNullOrEmpty(campanna.VideoArchivo.Ruta))
                return NotFound(new { message = "No se ha encontrado el archivo de video" });

            var videoFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", campanna.VideoArchivo.Ruta);

            if (!System.IO.File.Exists(videoFilePath))
                return NotFound(new { message = "El archivo de video no se encuentra en el servidor" });

            var fileBytes = await System.IO.File.ReadAllBytesAsync(videoFilePath);

            return File(fileBytes, "video/mp4", campanna.VideoArchivo.Nombre);
        }

    }
}
