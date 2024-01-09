using System.Diagnostics;

namespace StarCitizenPlaytime;

class Program
{
    const int BufferSize = 1024;
    static void Main(string[] args)
    {
        var sw = Stopwatch.StartNew();
        
        var files = Directory.EnumerateFiles("data", "*.log");
        
        var playtimes = files.AsParallel().Select(GetPlaytimeFast).ToList();
        
        var totalPlaytime = playtimes.Aggregate(TimeSpan.Zero, (a, b) => a + b);
        
        sw.Stop();
        
        Console.WriteLine($"Total playtime: {totalPlaytime.TotalHours}. Took {sw.ElapsedMilliseconds}ms");
    }

    private static TimeSpan GetPlaytimeFast(string arg)
    {
        using var reader = new StreamReader(arg);
        var startDate = ReadFromStart(reader);
        var endDate = ReadFromEnd(reader);
        return endDate - startDate;
    }

    private static DateTime ReadFromStart(StreamReader reader)
    {
        var dateSpan = "<20".AsSpan();
        Span<char> buffer = stackalloc char[BufferSize];
        var index = -1;
        var offset = 0;
        
        while (index == -1 && reader.BaseStream.Position < reader.BaseStream.Length)
        {
            var read = reader.Read(buffer.Slice(offset, BufferSize));
            offset += read;
            index = buffer.IndexOf(dateSpan);
        }
        
        if (index == -1)
        {
            throw new Exception("Date not found");
        }
        
        return ParseDate(buffer.Slice(index + 1, 23));
    }
    
    private static DateTime ReadFromEnd(StreamReader reader)
    {
        var dateSpan = "<20".AsSpan();
        Span<char> buffer = stackalloc char[BufferSize];
        var index = -1;
        var offset = BufferSize;
        
        while (index == -1 && reader.BaseStream.Position > 0)
        {
            reader.BaseStream.Seek(-1 * offset, SeekOrigin.End);
            var read = reader.Read(buffer[..BufferSize]);
            offset += read;
            index = buffer.IndexOf(dateSpan);
        }
        
        if (index == -1)
        {
            throw new Exception("Date not found");
        }
        
        return ParseDate(buffer.Slice(index + 1, 23));
    }

    private static DateTime ParseDate(Span<char> slice)
    {
        // return DateTime.Parse(slice);
        //incoming format: 2023-11-22T10:13:02.987Z
        
        var year = int.Parse(slice.Slice(0, 4));
        var month = int.Parse(slice.Slice(5, 2));
        var day = int.Parse(slice.Slice(8, 2));
        var hour = int.Parse(slice.Slice(11, 2));
        var minute = int.Parse(slice.Slice(14, 2));
        var second = int.Parse(slice.Slice(17, 2));
        
        return new DateTime(year, month, day, hour, minute, second);
    }
}