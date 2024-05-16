using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml;

namespace FileProcessor.BackgroundService
{
    public class FileProcessorService
    {
        private readonly ILogger<FileProcessorService> _logger;
        string inputFolderDirectory = "C:\\Input";
        public FileProcessorService(ILogger<FileProcessorService> logger)
        {
            this._logger = logger;
        }

        //background service that checks for new files created
        //checks if the newly created file has the same name as the a corresponding pdf file
        //if not, check thrice: then if not, log failure
        //if it has the same name (matches) as the corresponding pdf file, 
        //proceed to process the xml file in the following steps
        //open the xml file and convert the node values and names to a C# object
        //copy the pdf to another folder named "Output" nameing the file in the format mrn_documentType_encounterDate NB ecnpunterdate format is dd-mm-yyyy e.g. 1234567D_LCVGP_07-05-2024.pdf
        //once this is done, i.e. pdf file has been placed into the output folder.
        //remove/delete the xml and pdf from the input folder

        public void FileWatcher()
        {
            

            FileSystemWatcher watcher = new FileSystemWatcher(inputFolderDirectory);

            watcher.Created += FileCreated;
            watcher.EnableRaisingEvents = true;
        }

        public void FileCreated(object sender, FileSystemEventArgs e)
        {
            //string correspondingFileName = Path.GetFileName(@"C:\Input\test.pdf");
            
            //string filePath = Path.GetFullPath(e.FullPath);
            string fileExtension = Path.GetExtension(e.FullPath);
           
            string baseFileName = Path.GetFileNameWithoutExtension(e.FullPath);
            string correspondingFilePath = Path.Combine(inputFolderDirectory, baseFileName + ".pdf");


            int retryCount = 0;
            int maximumRetryCount = 3;

          
            if (fileExtension == ".xml")
            {
                while((File.Exists(correspondingFilePath) == false) && (retryCount < maximumRetryCount))
                {
                    retryCount++;
                    _logger.LogInformation("Corresponding file not found. Retry Attempt "+ retryCount + " of "+ maximumRetryCount);
                }

                if ( File.Exists(correspondingFilePath) == false)
                {
                    _logger.LogError($"Corresponding PDF File {baseFileName} not found after 3 trialsin the directory {correspondingFilePath}");
                }

                ProcessXMLFile(e.FullPath);
            }


        }

        private void ProcessXMLFile(string filePath)
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(filePath);

            XmlNode mrnNode = xmlDoc.SelectSingleNode("medisight/mrn");
            XmlNode encounterDateNode = xmlDoc.SelectSingleNode("medisight/encounterDate");
            XmlNode documentTypeNode = xmlDoc.SelectSingleNode("medisight/documentType");

            DateTime encouterDate = DateTime.ParseExact(encounterDateNode.InnerText.ToString(), "yyyy/MM/dd hh:mm:ss tt", CultureInfo.InvariantCulture);
            var encounterDateddmmyyFormat = encouterDate.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);

            var xmlObject = new XMLObject
            {
                mrn = mrnNode.InnerText,
                documentType = documentTypeNode.InnerText,
                encounterDate = encounterDateddmmyyFormat
            };

           
            
        }
    }
}
