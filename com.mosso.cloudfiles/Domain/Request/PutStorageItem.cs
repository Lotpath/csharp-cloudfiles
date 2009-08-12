///
/// See COPYING file for licensing information
///

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using com.mosso.cloudfiles.domain.request.Interfaces;
using com.mosso.cloudfiles.exceptions;
using com.mosso.cloudfiles.utils;

namespace com.mosso.cloudfiles.domain.request
{
    /// <summary>
    /// PutStorageItem
    /// </summary>
    public class PutStorageItem : IAddToWebRequest
    {
        public Stream Stream { get; set; }
        private readonly string _storageUrl;
        private readonly string _containerName;
        private readonly string _remoteStorageItemName;
        private Stream stream;
        private readonly Dictionary<string, string> _metadata;
        private string _fileUrl;
       

        public event Connection.ProgressCallback Progress;

        /// <summary>
        /// PutStorageItem constructor
        /// </summary>
        /// <param name="storageUrl">the customer unique url to interact with cloudfiles</param>
        /// <param name="containerName">the name of the container where the storage item is located</param>
        /// <param name="remoteStorageItemName">the name of the storage item to add meta information too</param>
        /// <param name="localFilePath">the path of the file to put into cloudfiles</param>
        public PutStorageItem(string storageUrl, string containerName, string remoteStorageItemName, string localFilePath)
            : this(storageUrl,  containerName, remoteStorageItemName, localFilePath, null)
        {
        }

        /// <summary>
        /// PutStorageItem constructor
        /// </summary>
        /// <param name="storageUrl">the customer unique url to interact with cloudfiles</param>
        /// <param name="containerName">the name of the container where the storage item is located</param>
        /// <param name="remoteStorageItemName">the name of the storage item to add meta information too</param>
        /// <param name="filestream">the fiel stream of the file to put into cloudfiles</param>
        public PutStorageItem(string storageUrl, string containerName, string remoteStorageItemName, Stream filestream)
            : this(storageUrl,  containerName, remoteStorageItemName, filestream, null)
        {
        }

        /// <summary>
        /// PutStorageItem constructor
        /// </summary>
        /// <param name="storageUrl">the customer unique url to interact with cloudfiles</param>
        /// <param name="containerName">the name of the container where the storage item is located</param>
        /// <param name="remoteStorageItemName">the name of the storage item to add meta information too</param>
        /// <param name="stream">the file stream of the file to put into cloudfiles</param>
        /// <param name="metadata">dictionary of meta tags to apply to the storage item</param>
        /// <exception cref="ArgumentNullException">Thrown when any of the reference parameters are null</exception>
        /// <exception cref="ContainerNameException">Thrown when the container name is invalid</exception>
        /// <exception cref="StorageItemNameException">Thrown when the object name is invalid</exception>
        public PutStorageItem(string storageUrl,  string containerName,
            string remoteStorageItemName,
            Stream stream,
            Dictionary<string, string> metadata)
        {
            Stream = stream;
            if (string.IsNullOrEmpty(storageUrl)
                || string.IsNullOrEmpty(containerName)
                || stream == null
                || string.IsNullOrEmpty(remoteStorageItemName))
                throw new ArgumentNullException();


            if (!ContainerNameValidator.Validate(containerName)) throw new ContainerNameException();
            if (!ObjectNameValidator.Validate(remoteStorageItemName)) throw new StorageItemNameException();

            _fileUrl = CleanUpFilePath(remoteStorageItemName);
            _storageUrl = storageUrl;
            _containerName = containerName;
            _remoteStorageItemName = remoteStorageItemName;
            this.stream = stream;
            _metadata = metadata;
//         
//            
//
//             _eTag = StringifyMD5(new MD5CryptoServiceProvider().ComputeHash(this.stream));
//
//            this.stream.Seek(0, 0);
//
//            if (metadata != null)
//            {
//                foreach (var s in metadata.Keys)
//                {
//                    Headers.Add(Constants.META_DATA_HEADER + s, metadata[s]);
//                }
//            }
//
//            if (stream.Position == stream.Length)
//                stream.Seek(0, 0);

            
        }

        /// <summary>
        /// PutStorageItem constructor
        /// </summary>
        /// <param name="storageUrl">the customer unique url to interact with cloudfiles</param>
        /// <param name="containerName">the name of the container where the storage item is located</param>
        /// <param name="remoteStorageItemName">the name of the storage item to add meta information too</param>
        /// <param name="localFilePath">the path of the file to put into cloudfiles</param>
        /// <param name="metadata">dictionary of meta tags to apply to the storage item</param>
        /// <exception cref="ArgumentNullException">Thrown when any of the reference parameters are null</exception>
        /// <exception cref="ContainerNameException">Thrown when the container name is invalid</exception>
        /// <exception cref="StorageItemNameException">Thrown when the object name is invalid</exception>
        public PutStorageItem(string storageUrl, string containerName, string remoteStorageItemName, 
            string localFilePath,
            Dictionary<string, string> metadata)
        {
            _storageUrl = storageUrl;
            _containerName = containerName;
            _remoteStorageItemName = remoteStorageItemName;
            _metadata = metadata;
            if (string.IsNullOrEmpty(storageUrl)
                || string.IsNullOrEmpty(containerName)
                || string.IsNullOrEmpty(localFilePath)
                || string.IsNullOrEmpty(remoteStorageItemName))
                throw new ArgumentNullException();


            if (!ContainerNameValidator.Validate(containerName)) throw new ContainerNameException();
            if (!ObjectNameValidator.Validate(remoteStorageItemName))throw new StorageItemNameException();
            
            _fileUrl  = CleanUpFilePath(localFilePath);
          

            
            
        }
       

//        /// <summary>
//        /// inputs the supplied file stream into the http request
//        /// </summary>
//        /// <param name="httpWebRequestFileStream">the file stream to input into the http request</param>
//        public void ReadFileIntoRequest(Stream httpWebRequestFileStream)
//        {
//            if (stream == null)
//                stream = new FileStream(FileUri, FileMode.Open);
//
//            ReadStreamIntoRequest(httpWebRequestFileStream);
//            stream.Close();
//        }

        /// <summary>
        /// the entity tag of the storage item
        /// </summary>
        /// <returns>string representation of the entity tag</returns>
//        public string ETag
//        {
//            get { return Headers[Constants.ETAG]; }
//            private set { Headers.Add(Constants.ETAG, value); }
//        }

        /// <summary>
        /// the content type of the storage item
        /// </summary>
        /// <returns>string representation of the storage item's content type</returns>
        private string ContentType()
        {
             
                if (String.IsNullOrEmpty(_fileUrl) || _fileUrl.IndexOf(".") < 0) return "application/octet-stream";
                return MimeType(_fileUrl);
           
        }
        #region private methods so big I need to use regions
        private string MimeType(string filename)
        {
            string mime;

            switch (Path.GetExtension(filename).ToLower())
            {
                case ".3dm": mime = "x-world/x-3dmf"; break;
                case ".3dmf": mime = "x-world/x-3dmf"; break;
                case ".a": mime = "application/octet-stream"; break;
                case ".aab": mime = "application/x-authorware-bin"; break;
                case ".aam": mime = "application/x-authorware-map"; break;
                case ".aas": mime = "application/x-authorware-seg"; break;
                case ".abc": mime = "text/vnd.abc"; break;
                case ".acgi": mime = "text/html"; break;
                case ".afl": mime = "video/animaflex"; break;
                case ".ai": mime = "application/postscript"; break;
                case ".aif": mime = "audio/aiff"; break;
                case ".aifc": mime = "audio/aiff"; break;
                case ".aiff": mime = "audio/aiff"; break;
                case ".aim": mime = "application/x-aim"; break;
                case ".aip": mime = "text/x-audiosoft-intra"; break;
                case ".ani": mime = "application/x-navi-animation"; break;
                case ".aos": mime = "application/x-nokia-9000-communicator-add-on-software"; break;
                case ".aps": mime = "application/mime"; break;
                case ".arc": mime = "application/octet-stream"; break;
                case ".arj": mime = "application/arj"; break;
                case ".art": mime = "image/x-jg"; break;
                case ".asf": mime = "video/x-ms-asf"; break;
                case ".asm": mime = "text/x-asm"; break;
                case ".asp": mime = "text/asp"; break;
                case ".asx": mime = "video/x-ms-asf"; break;
                case ".au": mime = "audio/basic"; break;
                case ".avi": mime = "video/avi"; break;
                case ".avs": mime = "video/avs-video"; break;
                case ".bcpio": mime = "application/x-bcpio"; break;
                case ".bin": mime = "application/octet-stream"; break;
                case ".bm": mime = "image/bmp"; break;
                case ".bmp": mime = "image/bmp"; break;
                case ".boo": mime = "application/book"; break;
                case ".book": mime = "application/book"; break;
                case ".boz": mime = "application/x-bzip2"; break;
                case ".bsh": mime = "application/x-bsh"; break;
                case ".bz": mime = "application/x-bzip"; break;
                case ".bz2": mime = "application/x-bzip2"; break;
                case ".c": mime = "text/plain"; break;
                case ".c++": mime = "text/plain"; break;
                case ".cat": mime = "application/vnd.ms-pki.seccat"; break;
                case ".cc": mime = "text/plain"; break;
                case ".ccad": mime = "application/clariscad"; break;
                case ".cco": mime = "application/x-cocoa"; break;
                case ".cdf": mime = "application/cdf"; break;
                case ".cer": mime = "application/pkix-cert"; break;
                case ".cha": mime = "application/x-chat"; break;
                case ".chat": mime = "application/x-chat"; break;
                case ".class": mime = "application/java"; break;
                case ".com": mime = "application/octet-stream"; break;
                case ".conf": mime = "text/plain"; break;
                case ".cpio": mime = "application/x-cpio"; break;
                case ".cpp": mime = "text/x-c"; break;
                case ".cpt": mime = "application/x-cpt"; break;
                case ".crl": mime = "application/pkcs-crl"; break;
                case ".crt": mime = "application/pkix-cert"; break;
                case ".csh": mime = "application/x-csh"; break;
                case ".css": mime = "text/css"; break;
                case ".cxx": mime = "text/plain"; break;
                case ".dcr": mime = "application/x-director"; break;
                case ".deepv": mime = "application/x-deepv"; break;
                case ".def": mime = "text/plain"; break;
                case ".der": mime = "application/x-x509-ca-cert"; break;
                case ".dif": mime = "video/x-dv"; break;
                case ".dir": mime = "application/x-director"; break;
                case ".dl": mime = "video/dl"; break;
                case ".doc": mime = "application/msword"; break;
                case ".dot": mime = "application/msword"; break;
                case ".dp": mime = "application/commonground"; break;
                case ".drw": mime = "application/drafting"; break;
                case ".dump": mime = "application/octet-stream"; break;
                case ".dv": mime = "video/x-dv"; break;
                case ".dvi": mime = "application/x-dvi"; break;
                case ".dwf": mime = "model/vnd.dwf"; break;
                case ".dwg": mime = "image/vnd.dwg"; break;
                case ".dxf": mime = "image/vnd.dwg"; break;
                case ".dxr": mime = "application/x-director"; break;
                case ".el": mime = "text/x-script.elisp"; break;
                case ".elc": mime = "application/x-elc"; break;
                case ".env": mime = "application/x-envoy"; break;
                case ".eps": mime = "application/postscript"; break;
                case ".es": mime = "application/x-esrehber"; break;
                case ".etx": mime = "text/x-setext"; break;
                case ".evy": mime = "application/envoy"; break;
                case ".exe": mime = "application/octet-stream"; break;
                case ".f": mime = "text/plain"; break;
                case ".f77": mime = "text/x-fortran"; break;
                case ".f90": mime = "text/plain"; break;
                case ".fdf": mime = "application/vnd.fdf"; break;
                case ".fif": mime = "image/fif"; break;
                case ".fli": mime = "video/fli"; break;
                case ".flo": mime = "image/florian"; break;
                case ".flx": mime = "text/vnd.fmi.flexstor"; break;
                case ".fmf": mime = "video/x-atomic3d-feature"; break;
                case ".for": mime = "text/x-fortran"; break;
                case ".fpx": mime = "image/vnd.fpx"; break;
                case ".frl": mime = "application/freeloader"; break;
                case ".funk": mime = "audio/make"; break;
                case ".g": mime = "text/plain"; break;
                case ".g3": mime = "image/g3fax"; break;
                case ".gif": mime = "image/gif"; break;
                case ".gl": mime = "video/gl"; break;
                case ".gsd": mime = "audio/x-gsm"; break;
                case ".gsm": mime = "audio/x-gsm"; break;
                case ".gsp": mime = "application/x-gsp"; break;
                case ".gss": mime = "application/x-gss"; break;
                case ".gtar": mime = "application/x-gtar"; break;
                case ".gz": mime = "application/x-gzip"; break;
                case ".gzip": mime = "application/x-gzip"; break;
                case ".h": mime = "text/plain"; break;
                case ".hdf": mime = "application/x-hdf"; break;
                case ".help": mime = "application/x-helpfile"; break;
                case ".hgl": mime = "application/vnd.hp-hpgl"; break;
                case ".hh": mime = "text/plain"; break;
                case ".hlb": mime = "text/x-script"; break;
                case ".hlp": mime = "application/hlp"; break;
                case ".hpg": mime = "application/vnd.hp-hpgl"; break;
                case ".hpgl": mime = "application/vnd.hp-hpgl"; break;
                case ".hqx": mime = "application/binhex"; break;
                case ".hta": mime = "application/hta"; break;
                case ".htc": mime = "text/x-component"; break;
                case ".htm": mime = "text/html"; break;
                case ".html": mime = "text/html"; break;
                case ".htmls": mime = "text/html"; break;
                case ".htt": mime = "text/webviewhtml"; break;
                case ".htx": mime = "text/html"; break;
                case ".ice": mime = "x-conference/x-cooltalk"; break;
                case ".ico": mime = "image/x-icon"; break;
                case ".idc": mime = "text/plain"; break;
                case ".ief": mime = "image/ief"; break;
                case ".iefs": mime = "image/ief"; break;
                case ".iges": mime = "application/iges"; break;
                case ".igs": mime = "application/iges"; break;
                case ".ima": mime = "application/x-ima"; break;
                case ".imap": mime = "application/x-httpd-imap"; break;
                case ".inf": mime = "application/inf"; break;
                case ".ins": mime = "application/x-internett-signup"; break;
                case ".ip": mime = "application/x-ip2"; break;
                case ".isu": mime = "video/x-isvideo"; break;
                case ".it": mime = "audio/it"; break;
                case ".iv": mime = "application/x-inventor"; break;
                case ".ivr": mime = "i-world/i-vrml"; break;
                case ".ivy": mime = "application/x-livescreen"; break;
                case ".jam": mime = "audio/x-jam"; break;
                case ".jav": mime = "text/plain"; break;
                case ".java": mime = "text/plain"; break;
                case ".jcm": mime = "application/x-java-commerce"; break;
                case ".jfif": mime = "image/jpeg"; break;
                case ".jfif-tbnl": mime = "image/jpeg"; break;
                case ".jpe": mime = "image/jpeg"; break;
                case ".jpeg": mime = "image/jpeg"; break;
                case ".jpg": mime = "image/jpeg"; break;
                case ".jps": mime = "image/x-jps"; break;
                case ".js": mime = "application/x-javascript"; break;
                case ".jut": mime = "image/jutvision"; break;
                case ".kar": mime = "audio/midi"; break;
                case ".ksh": mime = "application/x-ksh"; break;
                case ".la": mime = "audio/nspaudio"; break;
                case ".lam": mime = "audio/x-liveaudio"; break;
                case ".latex": mime = "application/x-latex"; break;
                case ".lha": mime = "application/octet-stream"; break;
                case ".lhx": mime = "application/octet-stream"; break;
                case ".list": mime = "text/plain"; break;
                case ".lma": mime = "audio/nspaudio"; break;
                case ".log": mime = "text/plain"; break;
                case ".lsp": mime = "application/x-lisp"; break;
                case ".lst": mime = "text/plain"; break;
                case ".lsx": mime = "text/x-la-asf"; break;
                case ".ltx": mime = "application/x-latex"; break;
                case ".lzh": mime = "application/octet-stream"; break;
                case ".lzx": mime = "application/octet-stream"; break;
                case ".m": mime = "text/plain"; break;
                case ".m1v": mime = "video/mpeg"; break;
                case ".m2a": mime = "audio/mpeg"; break;
                case ".m2v": mime = "video/mpeg"; break;
                case ".m3u": mime = "audio/x-mpequrl"; break;
                case ".man": mime = "application/x-troff-man"; break;
                case ".map": mime = "application/x-navimap"; break;
                case ".mar": mime = "text/plain"; break;
                case ".mbd": mime = "application/mbedlet"; break;
                case ".mc$": mime = "application/x-magic-cap-package-1.0"; break;
                case ".mcd": mime = "application/mcad"; break;
                case ".mcf": mime = "text/mcf"; break;
                case ".mcp": mime = "application/netmc"; break;
                case ".me": mime = "application/x-troff-me"; break;
                case ".mht": mime = "message/rfc822"; break;
                case ".mhtml": mime = "message/rfc822"; break;
                case ".mid": mime = "audio/midi"; break;
                case ".midi": mime = "audio/midi"; break;
                case ".mif": mime = "application/x-mif"; break;
                case ".mime": mime = "message/rfc822"; break;
                case ".mjf": mime = "audio/x-vnd.audioexplosion.mjuicemediafile"; break;
                case ".mjpg": mime = "video/x-motion-jpeg"; break;
                case ".mm": mime = "application/base64"; break;
                case ".mme": mime = "application/base64"; break;
                case ".mod": mime = "audio/mod"; break;
                case ".moov": mime = "video/quicktime"; break;
                case ".mov": mime = "video/quicktime"; break;
                case ".movie": mime = "video/x-sgi-movie"; break;
                case ".mp2": mime = "audio/mpeg"; break;
                case ".mp3": mime = "audio/mpeg"; break;
                case ".mpa": mime = "audio/mpeg"; break;
                case ".mpc": mime = "application/x-project"; break;
                case ".mpe": mime = "video/mpeg"; break;
                case ".mpeg": mime = "video/mpeg"; break;
                case ".mpg": mime = "video/mpeg"; break;
                case ".mpga": mime = "audio/mpeg"; break;
                case ".mpp": mime = "application/vnd.ms-project"; break;
                case ".mpt": mime = "application/vnd.ms-project"; break;
                case ".mpv": mime = "application/vnd.ms-project"; break;
                case ".mpx": mime = "application/vnd.ms-project"; break;
                case ".mrc": mime = "application/marc"; break;
                case ".ms": mime = "application/x-troff-ms"; break;
                case ".mv": mime = "video/x-sgi-movie"; break;
                case ".my": mime = "audio/make"; break;
                case ".mzz": mime = "application/x-vnd.audioexplosion.mzz"; break;
                case ".nap": mime = "image/naplps"; break;
                case ".naplps": mime = "image/naplps"; break;
                case ".nc": mime = "application/x-netcdf"; break;
                case ".ncm": mime = "application/vnd.nokia.configuration-message"; break;
                case ".nif": mime = "image/x-niff"; break;
                case ".niff": mime = "image/x-niff"; break;
                case ".nix": mime = "application/x-mix-transfer"; break;
                case ".nsc": mime = "application/x-conference"; break;
                case ".nvd": mime = "application/x-navidoc"; break;
                case ".o": mime = "application/octet-stream"; break;
                case ".oda": mime = "application/oda"; break;
                case ".omc": mime = "application/x-omc"; break;
                case ".omcd": mime = "application/x-omcdatamaker"; break;
                case ".omcr": mime = "application/x-omcregerator"; break;
                case ".p": mime = "text/x-pascal"; break;
                case ".p10": mime = "application/pkcs10"; break;
                case ".p12": mime = "application/pkcs-12"; break;
                case ".p7a": mime = "application/x-pkcs7-signature"; break;
                case ".p7c": mime = "application/pkcs7-mime"; break;
                case ".p7m": mime = "application/pkcs7-mime"; break;
                case ".p7r": mime = "application/x-pkcs7-certreqresp"; break;
                case ".p7s": mime = "application/pkcs7-signature"; break;
                case ".part": mime = "application/pro_eng"; break;
                case ".pas": mime = "text/pascal"; break;
                case ".pbm": mime = "image/x-portable-bitmap"; break;
                case ".pcl": mime = "application/vnd.hp-pcl"; break;
                case ".pct": mime = "image/x-pict"; break;
                case ".pcx": mime = "image/x-pcx"; break;
                case ".pdb": mime = "chemical/x-pdb"; break;
                case ".pdf": mime = "application/pdf"; break;
                case ".pfunk": mime = "audio/make"; break;
                case ".pgm": mime = "image/x-portable-greymap"; break;
                case ".pic": mime = "image/pict"; break;
                case ".pict": mime = "image/pict"; break;
                case ".pkg": mime = "application/x-newton-compatible-pkg"; break;
                case ".pko": mime = "application/vnd.ms-pki.pko"; break;
                case ".pl": mime = "text/plain"; break;
                case ".plx": mime = "application/x-pixclscript"; break;
                case ".pm": mime = "image/x-xpixmap"; break;
                case ".pm4": mime = "application/x-pagemaker"; break;
                case ".pm5": mime = "application/x-pagemaker"; break;
                case ".png": mime = "image/png"; break;
                case ".pnm": mime = "application/x-portable-anymap"; break;
                case ".pot": mime = "application/vnd.ms-powerpoint"; break;
                case ".pov": mime = "model/x-pov"; break;
                case ".ppa": mime = "application/vnd.ms-powerpoint"; break;
                case ".ppm": mime = "image/x-portable-pixmap"; break;
                case ".pps": mime = "application/vnd.ms-powerpoint"; break;
                case ".ppt": mime = "application/vnd.ms-powerpoint"; break;
                case ".ppz": mime = "application/vnd.ms-powerpoint"; break;
                case ".pre": mime = "application/x-freelance"; break;
                case ".prt": mime = "application/pro_eng"; break;
                case ".ps": mime = "application/postscript"; break;
                case ".psd": mime = "application/octet-stream"; break;
                case ".pvu": mime = "paleovu/x-pv"; break;
                case ".pwz": mime = "application/vnd.ms-powerpoint"; break;
                case ".py": mime = "text/x-script.phyton"; break;
                case ".pyc": mime = "applicaiton/x-bytecode.python"; break;
                case ".qcp": mime = "audio/vnd.qcelp"; break;
                case ".qd3": mime = "x-world/x-3dmf"; break;
                case ".qd3d": mime = "x-world/x-3dmf"; break;
                case ".qif": mime = "image/x-quicktime"; break;
                case ".qt": mime = "video/quicktime"; break;
                case ".qtc": mime = "video/x-qtc"; break;
                case ".qti": mime = "image/x-quicktime"; break;
                case ".qtif": mime = "image/x-quicktime"; break;
                case ".ra": mime = "audio/x-pn-realaudio"; break;
                case ".ram": mime = "audio/x-pn-realaudio"; break;
                case ".ras": mime = "application/x-cmu-raster"; break;
                case ".rast": mime = "image/cmu-raster"; break;
                case ".rexx": mime = "text/x-script.rexx"; break;
                case ".rf": mime = "image/vnd.rn-realflash"; break;
                case ".rgb": mime = "image/x-rgb"; break;
                case ".rm": mime = "application/vnd.rn-realmedia"; break;
                case ".rmi": mime = "audio/mid"; break;
                case ".rmm": mime = "audio/x-pn-realaudio"; break;
                case ".rmp": mime = "audio/x-pn-realaudio"; break;
                case ".rng": mime = "application/ringing-tones"; break;
                case ".rnx": mime = "application/vnd.rn-realplayer"; break;
                case ".roff": mime = "application/x-troff"; break;
                case ".rp": mime = "image/vnd.rn-realpix"; break;
                case ".rpm": mime = "audio/x-pn-realaudio-plugin"; break;
                case ".rt": mime = "text/richtext"; break;
                case ".rtf": mime = "text/richtext"; break;
                case ".rtx": mime = "text/richtext"; break;
                case ".rv": mime = "video/vnd.rn-realvideo"; break;
                case ".s": mime = "text/x-asm"; break;
                case ".s3m": mime = "audio/s3m"; break;
                case ".saveme": mime = "application/octet-stream"; break;
                case ".sbk": mime = "application/x-tbook"; break;
                case ".scm": mime = "application/x-lotusscreencam"; break;
                case ".sdml": mime = "text/plain"; break;
                case ".sdp": mime = "application/sdp"; break;
                case ".sdr": mime = "application/sounder"; break;
                case ".sea": mime = "application/sea"; break;
                case ".set": mime = "application/set"; break;
                case ".sgm": mime = "text/sgml"; break;
                case ".sgml": mime = "text/sgml"; break;
                case ".sh": mime = "application/x-sh"; break;
                case ".shar": mime = "application/x-shar"; break;
                case ".shtml": mime = "text/html"; break;
                case ".sid": mime = "audio/x-psid"; break;
                case ".sit": mime = "application/x-sit"; break;
                case ".skd": mime = "application/x-koan"; break;
                case ".skm": mime = "application/x-koan"; break;
                case ".skp": mime = "application/x-koan"; break;
                case ".skt": mime = "application/x-koan"; break;
                case ".sl": mime = "application/x-seelogo"; break;
                case ".smi": mime = "application/smil"; break;
                case ".smil": mime = "application/smil"; break;
                case ".snd": mime = "audio/basic"; break;
                case ".sol": mime = "application/solids"; break;
                case ".spc": mime = "text/x-speech"; break;
                case ".spl": mime = "application/futuresplash"; break;
                case ".spr": mime = "application/x-sprite"; break;
                case ".sprite": mime = "application/x-sprite"; break;
                case ".src": mime = "application/x-wais-source"; break;
                case ".ssi": mime = "text/x-server-parsed-html"; break;
                case ".ssm": mime = "application/streamingmedia"; break;
                case ".sst": mime = "application/vnd.ms-pki.certstore"; break;
                case ".step": mime = "application/step"; break;
                case ".stl": mime = "application/sla"; break;
                case ".stp": mime = "application/step"; break;
                case ".sv4cpio": mime = "application/x-sv4cpio"; break;
                case ".sv4crc": mime = "application/x-sv4crc"; break;
                case ".svf": mime = "image/vnd.dwg"; break;
                case ".svr": mime = "application/x-world"; break;
                case ".swf": mime = "application/x-shockwave-flash"; break;
                case ".t": mime = "application/x-troff"; break;
                case ".talk": mime = "text/x-speech"; break;
                case ".tar": mime = "application/x-tar"; break;
                case ".tbk": mime = "application/toolbook"; break;
                case ".tcl": mime = "application/x-tcl"; break;
                case ".tcsh": mime = "text/x-script.tcsh"; break;
                case ".tex": mime = "application/x-tex"; break;
                case ".texi": mime = "application/x-texinfo"; break;
                case ".texinfo": mime = "application/x-texinfo"; break;
                case ".text": mime = "text/plain"; break;
                case ".tgz": mime = "application/x-compressed"; break;
                case ".tif": mime = "image/tiff"; break;
                case ".tiff": mime = "image/tiff"; break;
                case ".tr": mime = "application/x-troff"; break;
                case ".tsi": mime = "audio/tsp-audio"; break;
                case ".tsp": mime = "application/dsptype"; break;
                case ".tsv": mime = "text/tab-separated-values"; break;
                case ".turbot": mime = "image/florian"; break;
                case ".txt": mime = "text/plain"; break;
                case ".uil": mime = "text/x-uil"; break;
                case ".uni": mime = "text/uri-list"; break;
                case ".unis": mime = "text/uri-list"; break;
                case ".unv": mime = "application/i-deas"; break;
                case ".uri": mime = "text/uri-list"; break;
                case ".uris": mime = "text/uri-list"; break;
                case ".ustar": mime = "application/x-ustar"; break;
                case ".uu": mime = "application/octet-stream"; break;
                case ".uue": mime = "text/x-uuencode"; break;
                case ".vcd": mime = "application/x-cdlink"; break;
                case ".vcs": mime = "text/x-vcalendar"; break;
                case ".vda": mime = "application/vda"; break;
                case ".vdo": mime = "video/vdo"; break;
                case ".vew": mime = "application/groupwise"; break;
                case ".viv": mime = "video/vivo"; break;
                case ".vivo": mime = "video/vivo"; break;
                case ".vmd": mime = "application/vocaltec-media-desc"; break;
                case ".vmf": mime = "application/vocaltec-media-file"; break;
                case ".voc": mime = "audio/voc"; break;
                case ".vos": mime = "video/vosaic"; break;
                case ".vox": mime = "audio/voxware"; break;
                case ".vqe": mime = "audio/x-twinvq-plugin"; break;
                case ".vqf": mime = "audio/x-twinvq"; break;
                case ".vql": mime = "audio/x-twinvq-plugin"; break;
                case ".vrml": mime = "application/x-vrml"; break;
                case ".vrt": mime = "x-world/x-vrt"; break;
                case ".vsd": mime = "application/x-visio"; break;
                case ".vst": mime = "application/x-visio"; break;
                case ".vsw": mime = "application/x-visio"; break;
                case ".w60": mime = "application/wordperfect6.0"; break;
                case ".w61": mime = "application/wordperfect6.1"; break;
                case ".w6w": mime = "application/msword"; break;
                case ".wav": mime = "audio/wav"; break;
                case ".wb1": mime = "application/x-qpro"; break;
                case ".wbmp": mime = "image/vnd.wap.wbmp"; break;
                case ".web": mime = "application/vnd.xara"; break;
                case ".wiz": mime = "application/msword"; break;
                case ".wk1": mime = "application/x-123"; break;
                case ".wmf": mime = "windows/metafile"; break;
                case ".wml": mime = "text/vnd.wap.wml"; break;
                case ".wmlc": mime = "application/vnd.wap.wmlc"; break;
                case ".wmls": mime = "text/vnd.wap.wmlscript"; break;
                case ".wmlsc": mime = "application/vnd.wap.wmlscriptc"; break;
                case ".word": mime = "application/msword"; break;
                case ".wp": mime = "application/wordperfect"; break;
                case ".wp5": mime = "application/wordperfect"; break;
                case ".wp6": mime = "application/wordperfect"; break;
                case ".wpd": mime = "application/wordperfect"; break;
                case ".wq1": mime = "application/x-lotus"; break;
                case ".wri": mime = "application/mswrite"; break;
                case ".wrl": mime = "application/x-world"; break;
                case ".wrz": mime = "x-world/x-vrml"; break;
                case ".wsc": mime = "text/scriplet"; break;
                case ".wsrc": mime = "application/x-wais-source"; break;
                case ".wtk": mime = "application/x-wintalk"; break;
                case ".xbm": mime = "image/x-xbitmap"; break;
                case ".xdr": mime = "video/x-amt-demorun"; break;
                case ".xgz": mime = "xgl/drawing"; break;
                case ".xif": mime = "image/vnd.xiff"; break;
                case ".xl": mime = "application/excel"; break;
                case ".xla": mime = "application/vnd.ms-excel"; break;
                case ".xlb": mime = "application/vnd.ms-excel"; break;
                case ".xlc": mime = "application/vnd.ms-excel"; break;
                case ".xld": mime = "application/vnd.ms-excel"; break;
                case ".xlk": mime = "application/vnd.ms-excel"; break;
                case ".xll": mime = "application/vnd.ms-excel"; break;
                case ".xlm": mime = "application/vnd.ms-excel"; break;
                case ".xls": mime = "application/vnd.ms-excel"; break;
                case ".xlt": mime = "application/vnd.ms-excel"; break;
                case ".xlv": mime = "application/vnd.ms-excel"; break;
                case ".xlw": mime = "application/vnd.ms-excel"; break;
                case ".xm": mime = "audio/xm"; break;
                case ".xml": mime = "application/xml"; break;
                case ".xmz": mime = "xgl/movie"; break;
                case ".xpix": mime = "application/x-vnd.ls-xpix"; break;
                case ".xpm": mime = "image/xpm"; break;
                case ".x-png": mime = "image/png"; break;
                case ".xsr": mime = "video/x-amt-showrun"; break;
                case ".xwd": mime = "image/x-xwd"; break;
                case ".xyz": mime = "chemical/x-pdb"; break;
                case ".z": mime = "application/x-compressed"; break;
                case ".zip": mime = "application/zip"; break;
                case ".zoo": mime = "application/octet-stream"; break;
                case ".zsh": mime = "text/x-script.zsh"; break;
                default: mime = "application/octet-stream"; break;
            }
            return mime;
        } 

    

        private static string StringifyMD5(byte[] bytes)
        {
            StringBuilder result = new StringBuilder();
            foreach (byte b in bytes)
                result.AppendFormat("{0:x2}", b);
            return result.ToString();
        }
        private string CleanUpFilePath(string filePath)
        {
            return filePath.StripSlashPrefix().Replace(@"file:\\\", "");
        }

        private void ReadStreamIntoRequest(Stream httpWebRequestFileStream)
        {
            byte[] buffer = new byte[Constants.CHUNK_SIZE];

            var amt = 0;
            while ((amt = stream.Read(buffer, 0, buffer.Length)) != 0)
            {
                httpWebRequestFileStream.Write(buffer, 0, amt);

                //Fire the progress event
                if (Progress != null)
                {
                    Progress(amt);
                }
            }

            stream.Close();
            httpWebRequestFileStream.Flush();
            httpWebRequestFileStream.Close();
        }
        #endregion 
        public Uri CreateUri()
        {
            
           return  new Uri(_storageUrl + "/" + _containerName.Encode() + "/" + _remoteStorageItemName.StripSlashPrefix().Encode());
        }

        public void Apply(ICloudFilesRequest request)
        {
            using (FileStream file = new FileStream(_fileUrl, FileMode.Open))
            {
                request.ContentType = this.ContentType();
                request.ContentLength = file.Length;
                request.ETag = StringifyMD5(new MD5CryptoServiceProvider().ComputeHash(file));
            }

            if (_metadata != null && _metadata.Count > 0)
            {
                foreach (var s in _metadata.Keys)
                {
                    request.Headers.Add(Constants.META_DATA_HEADER + s, _metadata[s]);
                }
            }
            if (stream == null)
                            stream = new FileStream(_fileUrl, FileMode.Open);
                            ReadStreamIntoRequest(stream);
                            stream.Close();
            if (stream.Position == stream.Length)
                stream.Seek(0, 0);

            
           request.Method = "PUT";
        }
    }
}