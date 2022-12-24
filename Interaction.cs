using Discord.WebSocket;
using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;

namespace DiscordImageGrabber
{
    class Interaction
    {
        DiscordSocketClient client;

        ulong firstMessageID = 850841160725037066;

        public Interaction(DiscordSocketClient cl)
        {
            client = cl;
        }

        public async Task<Task> StartInteraction()
        {
            await Task.Delay(1000);
            Console.Clear();
            //THE ROOT OF ALL INTERACTION COMPLETED BY THE USER.

            //Find guild
            ulong cGuild = GetGuild();
            SocketGuild sg = client.GetGuild(cGuild);

            //Find channel
            ulong cChannel = GetChannel(sg);
            SocketTextChannel sc = sg.GetTextChannel(cChannel);

            await GetImages(sc);

            return Task.CompletedTask;
        }

        public async Task<Task> GetImages(SocketTextChannel sc)
        {
            List<IMessage> retrievedMessages = new List<IMessage>();

            #region GET ALL IMAGE MESSAGES FROM THE CHANNEL
            IAsyncEnumerable<IReadOnlyCollection<IMessage>> msgs = sc.GetMessagesAsync();
            IEnumerable<IMessage> messageCollection = await msgs.FlattenAsync();

            foreach (IMessage msg in messageCollection)
            {
                //Console.WriteLine($"{msg.Author.Username} ::: {msg.Content}");
                retrievedMessages.Add(msg);
            }
            while (true)
            {
                Console.WriteLine($"FINISHED BATCH");
                Console.WriteLine($"COLLECTED MESSAGES: {retrievedMessages.Count()}");
                Console.WriteLine($"LAST MESSAGE: {retrievedMessages.Last().Content}");
                //Console.ReadLine();
                if (retrievedMessages.Count % 100 == 0)
                {
                    List<IMessage> newMessages = await RetrieveBatch(sc, retrievedMessages.Last());
                    foreach (IMessage msg in newMessages)
                    {
                        retrievedMessages.Add(msg);
                    }
                }
                else { break; }
            }
            Console.WriteLine("ALL MESSAGES RETRIEVED");
            #endregion

            //Output messages are in retrievedMessages

            //Filter to messages with attachments
            List<IMessage> filteredMessages = new List<IMessage>();
            foreach (IMessage msg in retrievedMessages.Where(msg => msg.Attachments.Count() > 0))
            {
                filteredMessages.Add(msg);
            }

            List<ImageDetails> dcImages = new List<ImageDetails>();
            //Go through each filtered message
            foreach (IMessage msg in filteredMessages)
            {
                //Find all attachments of type image
                foreach (IAttachment attachment in msg.Attachments)
                {
                    if (attachment.ContentType.Contains("image"))
                    {
                        dcImages.Add(new ImageDetails(attachment, msg.CreatedAt));
                    }
                }
            }

            Console.WriteLine("Total images found: " + dcImages.Count());

            using (WebClient client = new WebClient())
            {
                int i = 0;
                foreach (ImageDetails id in dcImages)
                {
                    try
                    {
                        client.DownloadFile(new Uri(id.attachment.Url), $"dumped/{id.date.ToString("dd_MM_yyyy_hhmm")}_{i}.png");
                        Console.WriteLine($"File {i} saved as dumped/{id.date.ToString("dd_MM_yyyy_hhmm")}_{i}.png     MIME TYPE {id.attachment.ContentType}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Exception with {i} > {ex.Message}");
                    }
                    i++;
                }
            }

            return Task.CompletedTask;
        }

        public async Task<List<IMessage>> RetrieveBatch(SocketTextChannel channel, IMessage lastMessage)
        {
            List<IMessage> currentBatch = new List<IMessage>();
            IAsyncEnumerable<IReadOnlyCollection<IMessage>> msgs = channel.GetMessagesAsync(fromMessage: lastMessage, dir: Direction.Before);
            IEnumerable<IMessage> messageCollection = await msgs.FlattenAsync();

            foreach (IMessage msg in messageCollection)
            {
                //Console.WriteLine($"{msg.Author.Username} ::: {msg.Content}");
                currentBatch.Add(msg);
            }

            return currentBatch;
        }

        public ulong GetGuild()
        {
            IReadOnlyCollection<SocketGuild> guilds = client.Guilds;
            int index = 0;
            foreach (SocketGuild guild in guilds)
            {
                Console.WriteLine($"{index} --- {guild.Name}");
                index++;
            }
            Console.WriteLine("Enter index to use");
            int selection = Int32.Parse(Console.ReadLine());
            return (guilds.ElementAt(selection).Id);
        }

        public ulong GetChannel(SocketGuild sg)
        {
            List<SocketGuildChannel> channels = new List<SocketGuildChannel>();
            foreach (SocketGuildChannel channel in sg.Channels.Where(sc => sc.GetChannelType() == ChannelType.Text))
            {
                channels.Add(channel);
            }
            int index = 0;
            foreach (SocketGuildChannel channel in sg.Channels.Where(sc => sc.GetChannelType() == ChannelType.Text))
            {
                Console.WriteLine($"{index} --- {channel.Name}");
                index++;
            }

            Console.WriteLine("Enter index to use");
            int selection = Int32.Parse(Console.ReadLine());
            return (channels.ElementAt(selection).Id);
        }
    }

    class ImageDetails
    {
        public IAttachment attachment { get; set; }
        public DateTimeOffset date { get; set; }

        public ImageDetails(IAttachment attachment, DateTimeOffset date)
        {
            this.attachment = attachment;
            this.date = date;
        }
    }
}
