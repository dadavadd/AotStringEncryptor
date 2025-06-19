# AotStringEncryptor 🛡️

Tired of your precious strings—API keys, connection strings, secret messages—lying around in your compiled binaries for anyone to see? Stop making it easy for them. **AotStringEncryptor** is a modern C# Source Generator that encrypts your strings at **compile-time**, making them invisible to casual snooping and basic reverse-engineering tools.

It's fast, NativeAOT friendly, and requires zero effort.

![-----------------------------------------------------](https://raw.githubusercontent.com/andreasbm/readme/master/assets/lines/rainbow.png)

### ✨ Why You Need This

*   🔐 **Compile-Time Encryption**: Your strings are encrypted before your app is even built. The original plain text never makes it into the final assembly.
*   🚀 **AOT & Trimming Friendly**: No reflection, no runtime lookups. Perfect for modern .NET applications, including Native AOT, MAUI, and Blazor.
*   ✍️ **Ridiculously Simple**: Just put your strings in a text file. The generator handles the rest.
*   🤫 **Stay Under the Radar**: The decryption logic is a simple, non-obvious routine embedded directly in your code, making it harder to spot and analyze than a call to a standard decryption library.

![-----------------------------------------------------](https://raw.githubusercontent.com/andreasbm/readme/master/assets/lines/rainbow.png)

### ⚙️ How It Works

You're smart, you get it. But here's the simple breakdown:

1.  You create a `Strings.txt` file with your key-value pairs.
2.  During compilation, the generator reads this file.
3.  It generates a unique encryption key for each string based on its variable name.
4.  It encrypts the value and embeds the encrypted byte array and the key directly into a new C# file.
5.  It generates a static property like `EncryptedStrings.YourKey` that decrypts the string on the fly when you access it.

No one will find `"MySuperSecretPassword"` in your DLL. They'll just find a meaningless pile of bytes.

![-----------------------------------------------------](https://raw.githubusercontent.com/andreasbm/readme/master/assets/lines/rainbow.png)

### 🚀 Quick Start

Forget NuGet packages. Real control comes from owning the source.

#### 1. Get the Code

First, take the source code. Put it where you want it.

```bash
git clone https://github.com/your-username/AotStringEncryptor.git
```

#### 2. Integrate the Generator

Now, plug the generator directly into your project. You're not just a consumer; you're in command of the build process.

Reference the `AotStringEncryptor.csproj` from your main project. Using the command line is cleanest:

```bash
# Navigate to your project's directory
cd path/to/YourApp

# Add the reference. Adjust the path to where you cloned the generator.
dotnet add reference ../AotStringEncryptor/AotStringEncryptor.csproj
```
If you're a Visual Studio traditionalist, you can also do this: `Right-click Dependencies > Add Project Reference...` and select the `AotStringEncryptor.csproj`.

#### 3. Define Your Secrets

Create a file named `Strings.txt` in your project's root. This is your vault. Format is `key = value`.

**`Strings.txt`**
```
ApiKey = 12345-ABCDE-67890-FGHIJ
DatabaseConnection = Server=my_server;Database=my_db;User Id=my_user;Password=my_secret_password;
```

#### 4. Teach the Compiler

This is the part where you tell the compiler who's boss. You must mark `Strings.txt` so the generator can see it. **Don't skip this.**

In Visual Studio:
*   Right-click `Strings.txt` in the Solution Explorer.
*   Go to `Properties`.
*   Set **Build Action** to **`C# analyzer additional file`**.

Or, for the pros, just add this to your `.csproj`:

```xml
<ItemGroup>
  <AdditionalFiles Include="Strings.txt" />
</ItemGroup>
```

#### 5. Access Your Secrets Safely

It's done. The generator now works for you. Access your secrets through the static `EncryptedStrings` class. They are decrypted only at the moment of use.

```csharp
using System;

public class MyService
{
    public void Connect()
    {
        var apiKey = EncryptedStrings.ApiKey; 
        var connection = EncryptedStrings.DatabaseConnection;

        Console.WriteLine($"Using API Key: {apiKey}");
        // ConnectToDatabase(connection);
    }
}
```

#### 6. The Magic Behind the Curtain

Curious what the generator produces? It creates a file like this in the background, embedding the encrypted data and decryption logic. This is what your compiler sees, not your plain text.

```csharp
using System;
using System.Text;
using System.Runtime.CompilerServices;

public static class EncryptedStrings
{
    public static string ApiKey => Decrypt(new byte[] { 0x7E, 0xEA, 0xAD, 0xF7, 0xA8, 0xC2, 0x1A, 0x1E, 0xC1, 0xD1, 0xE2, 0xED, 0xE6, 0xD3, 0x77, 0xC7, 0x0E, 0xE9, 0xA2, 0xF4, 0xA5, 0xB3, 0x7A }, new byte[] { 0x3C, 0xAE, 0xEB, 0xBF, 0xE8, 0xFC, 0x42, 0x44, 0x9D, 0x8F, 0x82, 0xDC, 0xD5, 0xE6, 0x40, 0xFE });
    public static string DatabaseConnection => Decrypt(new byte[] { 0x5B, 0x4F, 0x36, 0xA1, 0x0C, 0x7D, 0x25, 0xDF, 0x2C, 0x86, 0x91, 0xFB, 0xCB, 0x92, 0xF2, 0xEC, 0x79, 0xB0, 0xD6, 0xA5, 0x36, 0x47, 0x15, 0xEF, 0x2D, 0x5D, 0xB4, 0xDC, 0xCF, 0x87, 0xFC, 0xF0, 0x68, 0x95, 0x14, 0xB8, 0x2B, 0x5B, 0xE9, 0x2B, 0xF8, 0x95, 0x78, 0x1E, 0x2D, 0x40, 0xD8, 0x20, 0xAC, 0x5B, 0xDD, 0x80, 0xF6, 0x5C, 0xF1, 0x18, 0xFF, 0x6B, 0x64, 0x1E, 0x19, 0xBE, 0x24, 0x1E, 0x56, 0xBF, 0xD8, 0x82, 0x05, 0x87, 0xCF, 0xD9, 0x0B, 0x79, 0x55, 0x07 }, new byte[] { 0xD1, 0x2B, 0xA3, 0x03, 0x85, 0xDD, 0xBA, 0x4D, 0x8C, 0xEC, 0x11, 0x69, 0x6E, 0x34, 0x59, 0x48 });

...
}
