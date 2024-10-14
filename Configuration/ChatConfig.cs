using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Koboldcs.Configuration
{
    public class ChatConfig
    {
        private static ChatConfig _instance;

        public static ChatConfig Instance => _instance ??= new ChatConfig();

        private ChatConfig()
        {
            RootConfig = new Root();
        }

        public Root RootConfig { get; set; }

        public class Root
        {
            public string prompt { get; set; }
            public string memory { get; set; }
            public List<string> actions { get; set; }
            public Savedaestheticsettings savedaestheticsettings { get; set; }
            public Savedsettings savedsettings { get; set; }
        }

        public class Savedaestheticsettings
        {
            public string AI_portrait { get; set; }
        }

        public class Savedsettings
        {
            public string opmode { get; set; }


            public string chatname { get; set; }
            public string chatopponent { get; set; }
            public string instruct_starttag { get; set; }
            public string instruct_endtag { get; set; }
            public string instruct_systag { get; set; }
            public string instruct_sysprompt { get; set; }


            public int max_context_length { get; set; }
            public int max_length { get; set; }
            public double rep_pen { get; set; }
            public double temperature { get; set; }
            public double top_p { get; set; }
            public float min_p { get; set; }
            public float presence_penalty { get; set; }
            public int top_k { get; set; }
            public int top_a { get; set; }
            public float typ_s { get; set; }
            public float tfs_s { get; set; }
            public int miro_type { get; set; }
            public float miro_tau { get; set; }
            public double miro_eta { get; set; }
        }
    }
}
