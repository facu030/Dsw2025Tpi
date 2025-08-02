using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dsw2025Tpi.Domain.Enums
{

    /*
     el OrderStatus se utiliza para representar el estado actual de un pedido en particular.
    PENDING pendiente
    PROCESSING en  proceso
    SHIPPED enviado
    DELIVERED entregado
    CANCELLED cancelado
     */
    public enum OrderStatus 
    {

        PENDING,
        PROCESSING,
        SHIPPED,
        DELIVERED,
        CANCELLED



    }
}
