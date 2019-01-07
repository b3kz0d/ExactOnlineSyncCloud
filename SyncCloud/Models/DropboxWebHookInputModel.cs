namespace SyncCloud.Models
{
    public class DropboxWebHookInputModel
    {
        public ListFolder ListFolder { get; set; }
        public Delta Delta { get; set; }
    }

    public class Delta
    {
        public int[] Users { get; set; }
    }

    public class ListFolder
    {
        public string[] Accounts { get; set; }
        
    }
}