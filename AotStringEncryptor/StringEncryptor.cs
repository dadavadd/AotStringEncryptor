using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Linq;
using System.Text;

[Generator]
public class StringEncryptor : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var additionalFiles = context.AdditionalTextsProvider
            .Where(file => file.Path.EndsWith("Strings.txt"))
            .Combine(context.AnalyzerConfigOptionsProvider);

        context.RegisterSourceOutput(additionalFiles, (spc, tuple) =>
        {
            var (file, _) = tuple;
            var lines = file.GetText()?.Lines;
            if (lines == null) return;

            var fields = new StringBuilder();

            foreach (var line in lines)
            {
                var text = line.ToString().Trim();
                if (string.IsNullOrWhiteSpace(text) || !text.Contains("=")) continue;

                var split = text.Split(new[] { "=" }, 2, StringSplitOptions.None);
                var name = split[0].Trim();
                var value = split[1].Trim();

                var keyBytes = GenerateKey(name, 16);
                var encrypted = EncryptString(value, keyBytes);
                var byteArray = string.Join(", ", encrypted.Select(b => "0x" + b.ToString("X2")));
                var keyArray = string.Join(", ", keyBytes.Select(b => "0x" + b.ToString("X2")));

                fields.AppendLine($"    public static string {name} => Decrypt(new byte[] {{ {byteArray} }}, new byte[] {{ {keyArray} }});");
            }

            var source = $@"
using System;
using System.Text;
using System.Runtime.CompilerServices;

public static class EncryptedStrings
{{
{fields}
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static string Decrypt(byte[] data, byte[] key)
    {{
        var buffer = new byte[data.Length];
        
        for (int i = 0; i < data.Length; i++)
        {{
            var originalPos = (i - data.Length / 2 + data.Length) % data.Length;
            buffer[originalPos] = (byte)(data[i] ^ key[i % key.Length]);
        }}
        
        for (int i = 0; i < buffer.Length; i++)
            buffer[i] = (byte)((buffer[i] - (i % 256) + 256) % 256);
        
        return Encoding.UTF8.GetString(buffer);
    }}
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static uint Hash(string input)
    {{
        uint hash = 2166136261u;
        foreach (char c in input)
        {{
            hash ^= c;
            hash *= 16777619u;
        }}
        return hash;
    }}
}}";
            spc.AddSource("EncryptedStrings.g.cs", SourceText.From(source, Encoding.UTF8));
        });
    }

    private static byte[] GenerateKey(string seed, int length)
    {
        uint state = Hash(seed);
        var key = new byte[length];

        for (int i = 0; i < length; i++)
        {
            state ^= state << 13;
            state ^= state >> 17;
            state ^= state << 5;

            key[i] = (byte)((state & 0xFF) == 0 ? 1 : (state & 0xFF));
        }

        return key;
    }

    private static byte[] EncryptString(string input, byte[] key)
    {
        var bytes = Encoding.UTF8.GetBytes(input);

        var step1 = new byte[bytes.Length];
        for (int i = 0; i < bytes.Length; i++)
            step1[i] = (byte)((bytes[i] + (i % 256)) % 256);

        var step2 = new byte[step1.Length];
        for (int i = 0; i < step1.Length; i++)
        {
            var newPos = (i + step1.Length / 2) % step1.Length;
            step2[newPos] = step1[i];
        }

        var result = new byte[step2.Length];
        for (int i = 0; i < step2.Length; i++)
            result[i] = (byte)(step2[i] ^ key[i % key.Length]);

        return result;
    }

    private static uint Hash(string input)
    {
        uint hash = 2166136261u;
        foreach (char c in input)
        {
            hash ^= c;
            hash *= 16777619u;
        }
        return hash;
    }
}