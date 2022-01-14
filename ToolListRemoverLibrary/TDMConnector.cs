using Dapper;
using Microsoft.Data.SqlClient;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;

namespace ToolListRemoverLibrary
{
    public class TDMConnector
    {
        private static string GetConnectionString(string name) => ConfigurationManager.ConnectionStrings[name].ConnectionString;
        private static IDbConnection GetTDMConnection() => new SqlConnection(GetConnectionString("TDM PROD"));
        public static void DeleteNcPrograms(List<string> listsIds)
        {
            foreach (string listId in listsIds)
            {
                // Get List of NC programs with file locations
                List<string> filePaths = GetNcFilesPaths(listId);
                // Delete files ignoring exception if file is not found
                foreach (string filePath in filePaths)
                {
                    try
                    {
                        File.Delete(filePath);
                    }
                    catch (FileNotFoundException)
                    {
                        ;
                    }
                }
                // Delete db entries
                DeleteNcProgramsDbData(listId);
            }
        }

        public static List<string> VerifyListsIds(List<string> listsIds)
        {
            List<string> verifiedListsIds = new();
            using IDbConnection cnxn = GetTDMConnection();
            foreach (string listId in listsIds)
            {
                string id = cnxn.ExecuteScalar<string>($"SELECT LISTID FROM TDM_LIST WHERE LISTID = '{listId}'", commandType: CommandType.Text);
                if (id != null)
                {
                    verifiedListsIds.Add(id);
                }
            }
            return verifiedListsIds;
        }

        private static void DeleteNcProgramsDbData(string listId)
        {
            using IDbConnection cnxn = GetTDMConnection();
            cnxn.Execute($"DELETE FROM NCM_PRODDOCB WHERE LISTID = '{listId}'", commandType: CommandType.Text);
        }

        private static List<string> GetNcFilesPaths(string listId)
        {
            List<string> filePaths = new();
            using IDbConnection cnxn = GetTDMConnection();
            // Get Machine
            string machineId = GetMachineID(cnxn, listId);
            // Get file data
            List<NcProgramFileModel> ncPrograms = GetNcProgramsData(cnxn, listId);
            foreach (NcProgramFileModel ncProgram in ncPrograms)
            {
                // Set Machine for nc programs
                ncProgram.MachineId = machineId;
                // Get path for machine and status
                ncProgram.Path = GetNcProgramPath(cnxn, ncProgram.MachineId, ncProgram.StateId);
                // Create path for file
                filePaths.Add(CreateFilePath(ncProgram));
            }
            return filePaths;
        }

        private static string CreateFilePath(NcProgramFileModel ncProgram) =>
            ncProgram.Path + "\\" + ncProgram.FileId + "." + CreateFileVersionIndex(ncProgram.Version) + "." + ncProgram.Extension;

        private static string CreateFileVersionIndex(int version)
        {
            string index = version.ToString();
            while (index.Length < 4)
            {
                index = "0" + index;
            }
            return index;
        }

        private static string GetNcProgramPath(IDbConnection cnxn, string machineId, string stateId) =>
            cnxn.Query<string>($@"
SELECT PATH AS Path
FROM TDM_MACHINESTATEPATH
WHERE MACHINEID = '{machineId}' AND STATEID = '{stateId}'").First();

        private static List<NcProgramFileModel> GetNcProgramsData(IDbConnection cnxn, string listId) =>
            cnxn.Query<NcProgramFileModel>(@$"
SELECT FILEID AS FileId, EXTENSION AS Extension, STATEID AS StateId, VERSION AS Version
FROM NCM_PRODDOCB
WHERE LISTID = '{listId}'").ToList();

        private static string GetMachineID(IDbConnection cnxn, string listId) =>
            cnxn.Query<string>($"SELECT MACHINEID FROM TDM_LIST WHERE LISTID = '{listId}'", commandType: CommandType.Text).First();

        public static void DeleteToolLists(List<string> listsIds)
        {
            foreach (string listId in listsIds)
            {
                using IDbConnection cnxn = GetTDMConnection();
                // Delete positions
                cnxn.Execute($"DELETE FROM TDM_LISTLISTB WHERE LISTID = '{listId}'", commandType: CommandType.Text);
                // Delete Master Data
                cnxn.Execute($"DELETE FROM TDM_LIST WHERE LISTID = '{listId}'", commandType: CommandType.Text);
            }
        }
    }
}
