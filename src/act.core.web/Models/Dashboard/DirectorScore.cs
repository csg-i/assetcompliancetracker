namespace act.core.web.Models.Dashboard
{
    public class DirectorScore
    {
        public DirectorScore(string[] names, int[] passing, int[] failing, int[] notReporting)
        {
            Names = names;
            Passing = passing;
            Failing = failing;
            NotReporting = notReporting;
        }

        public string[] Names { get;  }
        public int[] Passing { get;  }
        public int[] Failing { get;  }
        public int[] NotReporting { get;  }
    }
}