
using System.Collections.ObjectModel;
using System.IO;
using System.Xml;
using System;

namespace ImageShrinker.ViewModels
{
    public class AddressBookViewModel : ViewModel
    {
        private const string XmlAttributeName = "Name";
        private const string XmlAttributeEMailAddress = "EMailAddress";
        private const string FileName = "addresses.xml";

        public ObservableCollection<AddressItem> AddressItems { get; private set; }

        private static string GetAddressBookFile()
        {
            string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "ImageShrinker");

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            return Path.Combine(path, FileName);
        }

        public AddressBookViewModel()
        {
            AddressItems = new ObservableCollection<AddressItem>();

            string file = GetAddressBookFile();

            if (File.Exists(file))
            {
                XmlDocument document = new XmlDocument();
                document.Load(file);

                if (document.DocumentElement.ChildNodes != null)
                {
                    foreach (XmlElement element in document.DocumentElement.ChildNodes)
                    {
                        AddressItems.Add(new AddressItem { Name = element.GetAttribute(XmlAttributeName), EMailAdress = element.GetAttribute(XmlAttributeEMailAddress) });
                    }
                }
            }
        }

        public void Save()
        {
            XmlDocument document = new XmlDocument();
            document.AppendChild(document.CreateElement("AddressBook"));

            foreach (AddressItem addressItem in AddressItems)
            {
                XmlElement element = (XmlElement)document.DocumentElement.AppendChild(document.CreateElement("AddressEntry"));
                element.SetAttribute(XmlAttributeName, addressItem.Name);
                element.SetAttribute(XmlAttributeEMailAddress, addressItem.EMailAdress);
            }

            document.Save(GetAddressBookFile());
        }
    }
}