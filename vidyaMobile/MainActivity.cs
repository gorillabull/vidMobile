using Android.App;
using Android.Widget;
using Android.OS;

using System.Net;
using System.Net.Http;
using System.IO;
using Java.IO;
using System;
using System.Threading.Tasks;


using Android.Security;


using Android.OS;



namespace vidyaMobile
{
    [Activity(Label = "vidyaMobile", MainLauncher = true)]
    public class MainActivity : Activity
    {
        Button b1;
        Button login;
        EditText uName;
        EditText pw;
        HttpClient client = new HttpClient();
        TextView status;
        string userId = "";
        int newSongs = 0;


        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            b1 = FindViewById<Button>(Resource.Id.button1);
            b1.Enabled = false;

            login = FindViewById<Button>(Resource.Id.button2);

            uName = FindViewById<EditText>(Resource.Id.editText1);
            pw = FindViewById<EditText>(Resource.Id.editText2);

            status = FindViewById<TextView>(Resource.Id.textView3);

            b1.Click += B1_Click;
            login.Click += Login_Click;
        }

        private void Login_Click(object sender, EventArgs e)
        {
            if (uName.Text.Length > 3)
            {
                if (pw.Text.Length > 3)
                {
                    userId = client.GetStringAsync("http://ec2-54-153-54-33.us-west-1.compute.amazonaws.com:8080/login&username=" + uName.Text + "&pw=" + pw.Text).Result;
                }
            }
            if (userId == "")
            {
                return; // not sure if this is safe lol 
            }
            else
            {
                newSongs = Convert.ToInt32(client.GetStringAsync("http://ec2-54-153-54-33.us-west-1.compute.amazonaws.com:8080/check&"+userId).Result); //check if the user has any new songs.


                b1.Enabled = true;              //enable the grab songs button!
                status.Text = "Greetings " + userId + "!" + "You have "+newSongs.ToString()+ " new songs!";
            }


        }

        private void B1_Click(object sender, System.EventArgs e)
        {
            //send a web request, get the data and write it to file. 


            //check with the server to see if any songs should be downloaded 
            string available_songs = client.GetStringAsync("http://ec2-54-153-54-33.us-west-1.compute.amazonaws.com:8080/getsonglist&" + userId).Result;

            if (available_songs == "NO_NEW_SONGS")
            {
                status.Text = "No Songs Available!";
                return;
            }

            string[] songnames = available_songs.Split(';');

            byte[] songData= new byte[] { 1, 2, 3 };

            Stream song = null;

            //get the folder path (where songs are saved)

            string base_folder_path = "";
            try
            {
                Java.IO.File[] dirs = GetExternalFilesDirs(null);

                foreach (Java.IO.File folder in dirs)
                {
                    bool isRemovable = Android.OS.Environment.InvokeIsExternalStorageRemovable(folder);
                    bool isEmulated = Android.OS.Environment.InvokeIsExternalStorageEmulated(folder);

                    if (isRemovable && !isEmulated)
                    {
                        base_folder_path = folder.Path;
                    }
                }
            }
            catch (Exception)
            {

                throw;
            }

            var sdcardPath = Android.OS.Environment.ExternalStorageDirectory.AbsolutePath;

            //download each song. 


            foreach (string item in songnames)
            {
                song = null;
                int tries = 0;
                while (song == null && tries < 4)
                {
                    try
                    {
                        song = client.GetStreamAsync("http://ec2-54-153-54-33.us-west-1.compute.amazonaws.com:8080/getsongs&"+item).Result;
                    }
                    catch (System.Exception)
                    {

                    }
                    tries++;
                }


                if (song!= null)
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        song.CopyTo(memoryStream);
                        songData = memoryStream.ToArray();
                    }

                    //save each song. 
                    var fPath = System.IO.Path.Combine(base_folder_path, item + ".mp3"); //test.mp3 has to be replaced lol 

                    var fstream = new FileStream(fPath, FileMode.Create);
                    fstream.Write(songData, 0, songData.Length);                    //this should write the binary file to disk 
                }


            }







            //TODO MAKE SURE APP CAN SAVE SONG DATA 






            //OutputStream os = new FileOutputStream(file);
            //SavetoSd();

        }


        private void SavetoSd()
        {
            var sdCardPath = Android.OS.Environment.ExternalStorageDirectory.Path;
            var filePath = System.IO.Path.Combine(sdCardPath, "iootext.txt");


            System.IO.File.Create(filePath);

            if (!System.IO.File.Exists(filePath))
            {
                //FileWriter wrt = new FileWriter()

                using (System.IO.StreamWriter write = new System.IO.StreamWriter(filePath, true))
                {
                    write.Write("hi there");
                }
            }
        }

        //TODO: MAKE SURE SONG DOWNLOADS, WRITE A FILE TO THE SD CARD, WRITE THE DOWNLOADED SONG TO THE CARD , WORK ON SERVER TO DL SONGS. 
    }
}

