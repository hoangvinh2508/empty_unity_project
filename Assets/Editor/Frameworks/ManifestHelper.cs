using System.Xml.Linq;
using UnityEngine;
using System.Xml;

public class ManifestHelper
{
    private XDocument doc;
    private XNamespace ns = @"http://schemas.android.com/apk/res/android";
    private XNamespace nsPackage = @"http://schemas.android.com/apk";
 
    public ManifestHelper(string path)
    {
        doc = XDocument.Load(path);
    }
 
    public void Save(string path)
    {
        doc.Save(path);
    }
 
    public void SetVersions(string versionName,int versionCode)
    {
        doc.Root.SetAttributeValue(ns + "versionCode", versionCode);
        doc.Root.SetAttributeValue(ns + "versionName", versionName);
    }

    public void SetPackageName(string packageName)
    {
        foreach (var attribute in doc.Root.Attributes())
        {
            if (attribute.Name == "package")
            {
                doc.Root.SetAttributeValue(attribute.Name, packageName);
            }
        }
    }

    public void SetFacebookApplication(string facebookAppId)
    {
        XElement xmlApplication = doc.Root.Element("application");
        XElement xmlMetaData = xmlApplication.Element("meta-data");
        XElement xmlProvider = xmlApplication.Element("provider");
        xmlMetaData.SetAttributeValue(ns + "value", "fb" + facebookAppId);
        xmlProvider.SetAttributeValue(ns + "authorities", "com.facebook.app.FacebookContentProvider" + facebookAppId);
    }
}