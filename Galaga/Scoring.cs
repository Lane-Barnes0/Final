using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Galaga
{
    public class Scoring
    {
        /// <summary>
        /// Have to have a default constructor for the XmlSerializer.Deserialize method
        /// </summary>
        public Scoring() { }

        /// <summary>
        /// Overloaded constructor used to create an object for long term storage
        /// </summary>
        /// <param name="score"></param>
       
        public Scoring(int score)
        {
            this.Name = "Default Player";
            this.Score = score;
            this.TimeStamp = DateTime.Now;
        }

        public string Name { get; set; }
        public int Score { get; set; }
       
        public DateTime TimeStamp { get; set; }
    }
}

