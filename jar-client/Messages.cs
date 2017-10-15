using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JAR.Client.Messages
{

    public class ActionMessage
    {
        public string action { get; set; }
    }
    public class NoticeMessage : ActionMessage
    {
        public string ctoken { get; set; }
        public string notice { get; set; }
        public NoticeMessage()
        {
            action = "notice";
        }
    }

    public class SubscribeMessage : ActionMessage
    {
        public string ctoken { get; set; }
        public SubscribeMessage()
        {
            action = "subscribe";
        }
        public SubscribeMessage(string ctoken) : this()
        {
            this.ctoken = ctoken;
        }
    }

    public class TickMessage : ActionMessage
    {
        public float CPUTotal { get; set; }
        public float MemoryUsed { get; set; }
        //public float AverageCores { get; set; }
        //public float[] Cores { get; set; }
        public string[] ctokens { get; set; }

        public TickMessage()
        {
            action = "performancetick";
        }

        public TickMessage(float CPUTotal, float MemoryUsed):this()//, float AverageCores, float[] Cores)
        {
            this.CPUTotal = CPUTotal;
            this.MemoryUsed = MemoryUsed;
            //this.AverageCores = AverageCores;
            //this.Cores = Cores;
        }
    }
}
