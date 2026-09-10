#if UNITY_ANDROID
using System.IO;
using System.Xml.Linq;
using UnityEditor.Android;

public sealed class QuestAndroidManifest : IPostGenerateGradleAndroidProject
{
    public int callbackOrder => 100;
    public void OnPostGenerateGradleAndroidProject(string path)
    {
        string manifest=Path.Combine(path,"src/main/AndroidManifest.xml");
        var document=XDocument.Load(manifest);XNamespace android="http://schemas.android.com/apk/res/android";
        var root=document.Root;
        root.Add(new XElement("uses-permission",new XAttribute(android+"name","android.permission.RECORD_AUDIO")));
        root.Element("application").SetAttributeValue(android+"networkSecurityConfig","@xml/bb8_network_security");
        document.Save(manifest);
        string resources=Path.Combine(path,"src/main/res/xml");Directory.CreateDirectory(resources);
        File.WriteAllText(Path.Combine(resources,"bb8_network_security.xml"),
            "<?xml version=\"1.0\" encoding=\"utf-8\"?><network-security-config><base-config cleartextTrafficPermitted=\"false\"/><domain-config cleartextTrafficPermitted=\"true\"><domain>127.0.0.1</domain><domain>localhost</domain></domain-config></network-security-config>");
    }
}
#endif
