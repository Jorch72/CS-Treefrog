﻿using System;

namespace Treefrog.Presentation.Commands
{
    public class CommandHistoryEventArgs : EventArgs
    {
        private CommandHistory _commandHistory;

        public CommandHistoryEventArgs (CommandHistory commandHistory)
        {
            _commandHistory = commandHistory;
        }

        public CommandHistory CommandHistory
        {
            get { return _commandHistory; }
        }
    }
}
