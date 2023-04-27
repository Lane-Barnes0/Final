using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Galaga
{
    public class UserInput
    {

        /// <summary>
        /// Have to have a default constructor for the XmlSerializer.Deserialize method
        /// </summary>
        public UserInput() { }

        /// <summary>
        /// Overloaded constructor used to create an object for long term storage
        /// </summary>
        /// <param name="keyBinds"></param>

        public UserInput(List<Keys> keyBinds)
        {  
            this.keyBinds = keyBinds;
        }

       
        public List<Keys> keyBinds { get; set; }

        
    }
}

