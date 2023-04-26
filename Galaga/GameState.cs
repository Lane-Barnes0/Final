using System;

namespace Galaga
{
    /// <summary>
    /// This class demonstrates how to create an object that can be serialized
    /// under the XNA framework.
    /// </summary>
    //[Serializable]
    public class GameState
    {
        /// <summary>
        /// Have to have a default constructor for the XmlSerializer.Deserialize method
        /// </summary>
        public GameState() { }

        /// <summary>
        /// Overloaded constructor used to create an object for long term storage
        /// </summary>
        /// <param name="score"></param>
        public GameState(uint score)
        {
            this.Name = "Default Player";
            this.Score = score;
            this.TimeStamp = DateTime.Now;
        }

        public string Name { get; set; }
        public uint Score { get; set; }
        public DateTime TimeStamp { get; set; }
    }
}
   