using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dsw2025Tpi.Application.Exceptions;

public  class NotFoundException : Exception
{
    //Se  utiliza  Para manejar casos especificos cuando no se encuentra un recurso
    public NotFoundException(string message) : base(message) 
    {
    
    
    }


}
