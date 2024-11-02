using System;
using System.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UndefinedBot.Core.NetWork;

namespace UndefinedBot.Core.Utils
{
    public enum ImageSendType
    {
        LocalFile = 0,
        Url = 1,
        Base64 = 2
    }
    public enum ImageSubType
    {
        Normal = 0,  //正常图片
        Emoji = 1,   //表情包
        Hot = 2,     //热图
        Battel = 3,  //斗图
    }
    public class MsgBuilder
    {
        private MsgBuilder() { }
        public static MsgBuilder GetInstance()
        {
            return new MsgBuilder();
        }

        private readonly List<JObject> _msgChain = [];

        public MsgBuilder Text(string text)
        {
            _msgChain.Add(new JObject()
            {
                { "type" , "text" },
                { "data" ,  new JObject(){
                    { "text" , text }
                } },
            });
            return this;
        }
        public MsgBuilder QFace(string face)
        {
            _msgChain.Add(new JObject()
            {
                { "type" , "text" },
                { "data" ,  new JObject(){
                    { "id" , face }
                } },
            });
            return this;
        }
        public MsgBuilder Image(string imageContent, ImageSendType sendType = ImageSendType.LocalFile,ImageSubType subtype = ImageSubType.Normal)
        {
            string FPrefix = "";
            if (sendType == ImageSendType.LocalFile)
            {
                FPrefix = "file:///";
            }
            else if (sendType == ImageSendType.Base64)
            {
                FPrefix = "base64://";
            }
            _msgChain.Add(new JObject()
            {
                { "type" , "image" },
                { "data" ,  new JObject(){
                    { "file" , $"{FPrefix}{imageContent}" },
                    { "subType" , (int)subtype }
                } },
            });
            return this;
        }
        public MsgBuilder At(long atUin)
        {
            _msgChain.Add(new JObject()
            {
                { "type" , "at" },
                { "data" ,  new JObject(){
                    { "qq" , atUin }
                } },
            });
            return this;
        }
        public MsgBuilder Reply(int msgId)
        {
            _msgChain.Add(new JObject()
            {
                { "type" , "reply" },
                { "data" ,  new JObject(){
                    { "id" , $"{msgId}" }
                } },
            });
            return this;
        }
        public List<JObject> Build()
        {
            return _msgChain;
        }
    }
}
