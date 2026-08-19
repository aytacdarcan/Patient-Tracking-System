using System.Collections.Generic;
using System;

namespace HastaTakip.Api.Dtos;

public class LabForVisitBulkDto
{
    public DateTime Tarih { get; set; }
    public List<LabItemDto> Items { get; set; } = new();
}