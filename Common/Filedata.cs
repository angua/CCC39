namespace Common;

public class Filedata
{
    public List<string> Lines = new List<string>();

    private Filedata()
    { }

    public Filedata(string path)
    {
        var datalist = File.ReadAllLines(path);
        foreach (var line in datalist)
        {
            Lines.Add(line);
        }
    }

    public static Filedata CreateFromString(string str)
    {
        var fileData = new Filedata();
        fileData.Lines.AddRange(str.Split("\r\n"));
        return fileData;
    }

    public static Filedata CreateFromStream(TextReader tr)
    {
        var fileData = new Filedata();
        string line;
        while ((line = tr.ReadLine()) != null)
        {
            fileData.Lines.Add(line);
        }
        return fileData;
    }


}
