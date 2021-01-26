using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OnlyOfficeDocumentClientNetCore.Model
{
    public enum TrackerStatus
    {
        NotFound = 0,
        Editing = 1,
        MustSave = 2,
        Corrupted = 3,
        Closed = 4,
    }
}
