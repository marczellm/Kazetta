﻿using System;

namespace Kazetta.ViewModel
{
    /// <summary>
    /// All the data that's saved to XML so that it's there for the next app launch
    /// </summary>
    [Serializable]
    public class AppData
    {
        public Student[] Students;
        public Teacher[] Teachers;
        public Group[] Groups;
    }
}
