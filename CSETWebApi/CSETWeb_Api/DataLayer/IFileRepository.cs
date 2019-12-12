//////////////////////////////// 
// 
//   Copyright 2018 Battelle Energy Alliance, LLC  
// 
// 
//////////////////////////////// 
using System.Collections.Generic;
using DataAccess.Model;
using DataLayer;

namespace DataAccess
{
    public interface IFileRepository
    {
        IEnumerable<FileDescriptionShort> AddFileDescriptions(FileResult fileResult);

        IEnumerable<FileDescriptionShort> GetAllFiles(int assessment_id);

        DOCUMENT_FILE GetFileDescription(int id);
    }
}


