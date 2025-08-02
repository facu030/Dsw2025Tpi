using Dsw2025Tpi.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dsw2025Tpi.Application.Dtos;

public  class UpdateOrderStatusRequest
{

    [Required]
    public OrderStatus NewStatus { get; set; }

}
