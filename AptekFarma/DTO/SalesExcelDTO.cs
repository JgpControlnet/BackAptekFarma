﻿using _AptekFarma.DTO;

namespace AptekFarma.DTO
{
    public class SalesExcelDTO
    {
        public IFormFile file { get; set; }
        public bool newCampaign { get; set; }
        public CampaignDTO? campaignDTO { get; set; }
    }
}