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

        EditText song_url_textbox;
        EditText song_name_textbox;
        Button add_to_queue_button;


        string userId = "";
        int newSongs = -1;
        string serverIp = "";


        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            b1 = FindViewById<Button>(Resource.Id.download_music);
            b1.Enabled = false;

            add_to_queue_button = FindViewById<Button>(Resource.Id.Add_to_queue);
            add_to_queue_button.Enabled = false;

            song_url_textbox = FindViewById<EditText>(Resource.Id.editText4);
            song_name_textbox = FindViewById<EditText>(Resource.Id.editText3);


            login = FindViewById<Button>(Resource.Id.login_button);

            uName = FindViewById<EditText>(Resource.Id.username);
            pw = FindViewById<EditText>(Resource.Id.editText2);

            status = FindViewById<TextView>(Resource.Id.textView3);

            b1.Click += B1_Click;
            login.Click += Login_Click;
            add_to_queue_button.Click += Add_to_queue_button_Click;
            int tries = 0;
            while (serverIp == "" && tries < 4)
            {
                try
                {
                    serverIp = client.GetStringAsync("http://www.leoambo.com/getip").Result; //botain the ip address of our server. 
                }
                catch (Exception)
                {

                }
                tries++;
            }
            if (tries >= 4)
            {
                status.Text = "could not connect error!";

            }
            serverIp.Trim();
        }

        private void Add_to_queue_button_Click(object sender, EventArgs e)
        {
            //send a request to the server to download from this url and name it this. 
            if (song_url_textbox.Text.Length > 1 && song_name_textbox.Text.Length > 1)
            {
                string ok = "";
                int tries = 0;
                while (ok == "" && tries < 4)
                {
                    try
                    {
                        FileObserver f;
                        
                        ok = client.GetStringAsync("http://" + serverIp + "/request_download&" + userId + "&" + song_url_textbox.Text + "&" + song_name_textbox.Text).Result;
                    }
                    catch (Exception)
                    {
                    }
                    tries++;
                }
                if (tries >= 3)
                {
                    return;
                }

            }
            status.Text = "Song requested, wait 10 minutes now.";
        }

        private void Login_Click(object sender, EventArgs e)
        {
            status.Text = "Attempting to log u in!";

            if (uName.Text.Length > 3)
            {
                if (pw.Text.Length > 3)
                {

                    int tries = 0;
                    while (userId == "" && tries < 4)
                    {
                        try
                        {
                            userId = client.GetStringAsync("http://" + serverIp + "/login&username=" + uName.Text + "&pw=" + pw.Text).Result;
                        }
                        catch (Exception)
                        {

                        }
                        tries++;
                    }
                    if (tries >= 4)
                    {
                        status.Text = "Error check your internets!";
                        return;
                    }
                }

            }
            else
            {
                status.Text = "Please enter a valid user name or and password!";
                return;
            }
            if (userId == "")
            {
                return; // not sure if this is safe lol 
            }
            else
            {
                int tries = 0;
                while (newSongs == -1 && tries < 4)
                {
                    try
                    {
                        newSongs = Convert.ToInt32(client.GetStringAsync("http://" + serverIp + "/check&" + userId).Result); //check if the user has any new songs.
                    }
                    catch (Exception)
                    {
                    }

                    tries++;

                }
                if (tries >= 4)
                {
                    status.Text = "error getting your songs please try again!";
                    return;
                }


                b1.Enabled = true;              //enable the grab songs button!
                add_to_queue_button.Enabled = true;
                status.Text = "Greetings " + userId + "!" + "You have " + newSongs.ToString() + " new songs!";
            }


        }

        /// <summary>
        /// Click this button to download the songs that you have.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void B1_Click(object sender, System.EventArgs e)
        {
            //send a web request, get the data and write it to file. 

            string available_songs = "";
            using (null)
            {
                //check with the server to see if any songs should be downloaded 

                int tries = 0;
                while (available_songs == "" && tries < 4)
                {
                    try
                    {
                        available_songs = client.GetStringAsync("http://" + serverIp + "/getsonglist&" + userId).Result;
                    }
                    catch (Exception)
                    {
                    }
                    tries++;
                }

                if (tries >= 4)
                {
                    status.Text = "could not get song list try again";
                    return;
                }
            }


            if (available_songs == "NO_NEW_SONGS")
            {
                status.Text = "No Songs Available!";
                return;
            }

            string[] songnames = available_songs.Split(';');


            //send a response to the server that we have received the song list.
            if (songnames.Length > 1)
            {
                string ok = "";
                int tries = 0;
                while (ok == "" && tries < 4)
                {
                    try
                    {
                        //dont forget the userid this is what the file is named after all... 
                        ok = client.GetStringAsync("http://" + serverIp + "/gotsongs_OK&" + userId).Result; //send the ok to the server to delete the songs list file. 
                    }
                    catch (Exception)
                    {
                    }
                    tries++;
                }
                if (tries >= 4)
                {
                    status.Text = status.Text + ":(";

                }

            }



            byte[] songData = new byte[] { 1, 2, 3 };

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

                //throw;
            }

            var sdcardPath = Android.OS.Environment.ExternalStorageDirectory.AbsolutePath;





            //download each song. 
            foreach (string item in songnames)// a song s name is url encoded to just send it , server has to decode it. 
            {
                song = null;
                if (item != "")
                {
                    int tries = 0;
                    while (song == null && tries < 4)
                    {
                        try
                        {
                            song = client.GetStreamAsync("http://" + serverIp + "/getsongs&" + item).Result;
                        }
                        catch (System.Exception)
                        {

                        }
                        tries++;
                    }
                }


                if (song != null)
                {
                    //write the song to memory(sd card)
                    using (var memoryStream = new MemoryStream())
                    {
                        song.CopyTo(memoryStream);
                        songData = memoryStream.ToArray();
                    }

                    //save the song. 
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

