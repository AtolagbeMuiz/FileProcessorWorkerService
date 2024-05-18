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
        string outputFolderDirectory = "C:\\Output";
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
            //get the newly created file details i.e extension and name
            string fileExtension = Path.GetExtension(e.FullPath);
           
            string baseFileName = Path.GetFileNameWithoutExtension(e.FullPath);

            string correspondingPDFFilePath = Path.Combine(inputFolderDirectory, baseFileName + ".pdf");


            int retryCount = 0;
            int maximumRetryCount = 3;

          
            if (fileExtension == ".xml")
            {
                while((File.Exists(correspondingPDFFilePath) == false) && (retryCount < maximumRetryCount))
                {
                    retryCount++;
                    _logger.LogInformation("Corresponding file not found. Retry Attempt "+ retryCount + " of "+ maximumRetryCount);
                }

                if ( File.Exists(correspondingPDFFilePath) == false)
                {
                    _logger.LogError($"Corresponding PDF File {baseFileName} not found after 3 trials in the directory {correspondingPDFFilePath}");
                    
                    return;
                }

                if(File.Exists(correspondingPDFFilePath) == true)
                {
                    ProcessXMLFile(e.FullPath, correspondingPDFFilePath);
                }
            }
            else
            {
                _logger.LogError("the newly created is not an XML File");

                return;
            }


        }

        private void ProcessXMLFile(string filePath, string correspondingPDFFilePath)
        {
            try
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(filePath);

                XmlNode mrnNode = xmlDoc.SelectSingleNode("medisight/mrn");
                XmlNode encounterDateNode = xmlDoc.SelectSingleNode("medisight/encounterDate");
                XmlNode documentTypeNode = xmlDoc.SelectSingleNode("medisight/documentType");

                DateTime encounterDateTme;
                DateTime.TryParseExact(encounterDateNode.InnerText, "yyyyMMddHHmmss", null,
                    System.Globalization.DateTimeStyles.None, out encounterDateTme);

                string formattedDate = encounterDateTme.ToString("dd-MM-yyyy");

                var xmlObject = new XMLObject
                {
                    mrn = mrnNode.InnerText,
                    documentType = documentTypeNode.InnerText,
                    encounterDate = formattedDate
                };


                var newPdfName = $"{xmlObject.mrn}_{xmlObject.documentType}_{xmlObject.encounterDate}.pdf";
                string newPdfFileDirectory = Path.Combine(outputFolderDirectory, newPdfName);

                //copy the the newly-renamed pdf file into the output directory
                File.Copy(correspondingPDFFilePath, newPdfFileDirectory, true);

                //delete files two files in the input folder directory
                File.Delete(filePath);
                File.Delete(correspondingPDFFilePath);

            }
            catch (Exception ex)
            {
                _logger.LogError($"Error message {ex.Message} with Inner exception {ex.InnerException} and stack trace {ex.StackTrace} occured " +
                                         $"while trying to process XML File");
                throw;

            }


        }
    }
}
