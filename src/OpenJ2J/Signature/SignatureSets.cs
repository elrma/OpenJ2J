using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenJ2J.Signature
{
    public class SignatureSets
    {
        private static SignatureSets? _instance = null;

        public static SignatureSets Instance
        {
            get => _instance ?? new SignatureSets();
            set => _instance = value;
        }

        public static List<SignatureSet> Sets { get; set; } = new List<SignatureSet>();

        public SignatureSets()
        {
            InitializeSignatureSets();
        }

        public void InitializeSignatureSets()
        {
            AddSignatureSet("ICO File Format (ICO)", new string[] { ".ico" }, new string[] { "00000100" });

            AddSignatureSet("Compressed file(tar zip) using Lempel-Ziv-Welch algorithm (Z, TAR.Z)", new string[] { ".z", ".tar.z" }, new string[] { "1F9D" });

            AddSignatureSet("Compressed file(tar zip) using LZH algorithm (Z, TAR.Z)", new string[] { ".z", ".tar.z" }, new string[] { "1FA0" });

            AddSignatureSet("Compressed file using Bzip2 algorithm (BZ2)", new string[] { ".bz2" }, new string[] { "425A68" });

            AddSignatureSet("Graphics Interchange Format (GIF)", new string[] { ".gif" }, new string[] { "474946383761", "474946383961" });

            AddSignatureSet("Tagged Image File Format (TIFF)", new string[] { ".tif" }, new string[] { "49492A00", "4D4D002A" });

            AddSignatureSet("Image encoded in the JPEG raw or in the JFIF or EXIF file format (JPG, JPEG)", new string[] { ".jpg", ".jpeg" },
                new string[] { "FFD8FFDB", "FFD8FFE000104A4649460001", "FFD8FFEE", "FFD8FFE1" });
            
            AddSignatureSet("DOS MZ executable and its descendants (EXE..., Including NE and PE)",
                new string[] { ".exe", ".scr", ".sys", ".dll", ".fon", ".cpl", ".iec", ".ime", ".rs", ".tsp", ".mz" }, new string[] { "4D5A" });
            
            AddSignatureSet("DOS ZM executable and its descendants (EXE, Rare)", new string[] { ".exe" }, new string[] { "5A4D" });
            
            AddSignatureSet("ZIP file format and formats based on it, such as EPUB, JAR, ODF, OOXML (ZIP...)",
                new string[] { ".zip", ".aar", ".apk", ".docx", ".epub", ".ipa", ".jar", ".kmz", ".maff", ".msix", ".odp", ".ods", ".odt", ".pk3", ".pk4", ".pptx", ".usdz", ".vsdx", ".xlsx", ".xpi" },
                new string[] { "504B0304", "504B0506", "504B0708" });

            AddSignatureSet("Roshal ARchive compressed archive v1.50 onwards (RAR)", new string[] { ".rar" }, new string[] { "526172211A0700" });

            AddSignatureSet("Roshal ARchive compressed archive v5.00 onwards (RAR)", new string[] { ".rar" }, new string[] { "526172211A070100" });

            AddSignatureSet("Portable Network Graphics (PNG)", new string[] { ".png" }, new string[] { "89504E470D0A1A0A" });

            AddSignatureSet("PDF document (PDF)", new string[] { ".pdf" }, new string[] { "255044462D" });

            AddSignatureSet("Advanced Systems Format (ASF, WMA, WMV)", new string[] { ".asf", ".wma", ".wmv" }, new string[] { "3026B2758E66CF11A6D900AA0062CE6C" });

            AddSignatureSet("Open Source Media Container Format (OGG, OGA, OGV)", new string[] { ".ogg", ".oga", ".ogv" }, new string[] { "4F676753" });

            AddSignatureSet("Waveform Audio File Format (WAV, WAVE)", new string[] { ".wav", ".wave" }, new string[] { "57415645" }, 8);

            AddSignatureSet("Audio Video Interleave video format (AVI)", new string[] { ".avi" }, new string[] { "41564920" }, 8);

            AddSignatureSet("MPEG-1 Layer 3 file without an ID3 tag or with an ID3v1 tag (which is appended at the end of the file) (MP3)", new string[] { ".mp3" }, new string[] { "FFFB", "FFF3", "FFF2" });

            AddSignatureSet("MP3 file with an ID3v2 container (MP3)", new string[] { ".mp3" }, new string[] { "494433" });

            AddSignatureSet("Windows Bitmap Format (BMP)", new string[] { ".bmp", ".dib" }, new string[] { "424D" });

            AddSignatureSet("Free Lossless Audio Codec (FLAC)", new string[] { ".flac" }, new string[] { "664C6143" });

            AddSignatureSet("Musical Instrument Digital Interface (MID, MIDI)", new string[] { ".mid", ".midi" }, new string[] { "4D546864" });

            AddSignatureSet("eXtensible ARchive format archive (XAR)", new string[] { ".xar" }, new string[] { "78617221" });

            AddSignatureSet("Tape ARchiver archive (TAR)", new string[] { ".tar" }, new string[] { "7573746172003030", "7573746172202000" }, 257);

            AddSignatureSet("7-Zip compressed archive (7Z)", new string[] { ".7z" }, new string[] { "377ABCAF271C" });

            AddSignatureSet("GZIP compressed archive (GZ, TAR.GZ)", new string[] { ".gz", ".tar.gz" }, new string[] { "1F8B" });

            AddSignatureSet("XZ compressed archive using LZMA2 compression (XZ, TAR.XZ)", new string[] { ".xz", ".tar.xz" }, new string[] { "FD377A585A00" });

            AddSignatureSet("Matroska media container, including WebM (MKV, MKA, WEBM...)", new string[] { ".mkv", ".mka", ".mks", ".mk3d", ".webm" }, new string[] { "1A45DFA3" });

            AddSignatureSet("Google WebP image file (WEBP)", new string[] { ".webp" }, new string[] { "57454250" }, 8);

            AddSignatureSet("ISO Base Media file (MPEG-4) (MP4)", new string[] { ".mp4" }, new string[] { "6674797069736F6D" }, 4);
        }

        /// <summary>
        /// Add a signature set.
        /// </summary>
        /// <param name="name">The name of a signature set.</param>
        /// <param name="extensions">The extensions.</param>
        /// <param name="signatures">The signatures.</param>
        private void AddSignatureSet(string name, string[] extensions, string[] signatures, int offset = 0)
        {
            SignatureSet set = new SignatureSet();
            set.Name = name;
            set.Extensions = extensions.ToList();
            set.Signatures = signatures.ToList();
            set.Offset = offset;

            Sets.Add(set);
        }
    }
}
