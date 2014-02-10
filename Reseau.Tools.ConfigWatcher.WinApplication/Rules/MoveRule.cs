using System.Collections.Generic;

namespace Reseau.Tools.ConfigWatcher.Deamon.Rules
{
    public class MoveRule
    {
        private readonly string _pathContains;
        private readonly string _commentToPrepend;
        private readonly bool _savePreviousVersion;
        private readonly IDictionary<string, string> _movements;

        public MoveRule(string pathContains,
                        string commentToPrepend,
                        bool savePreviousVersion,
                        IDictionary<string, string> movements)
        {
            _pathContains = pathContains;
            _commentToPrepend = commentToPrepend;
            _savePreviousVersion = savePreviousVersion;
            _movements = movements;
        }

        public string PathContains
        {
            get { return _pathContains; }
        }

        public string CommentToPrepend
        {
            get { return _commentToPrepend; }
        }

        public bool SavePreviousVersion
        {
            get { return _savePreviousVersion; }
        }

        public IDictionary<string, string> Movements
        {
            get { return _movements; }
        }
    }
}