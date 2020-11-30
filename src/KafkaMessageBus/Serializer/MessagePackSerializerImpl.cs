using MessagePack;
using MessagePack.Resolvers;
using System;
using System.Collections.Generic;
using System.Text;

namespace Aix.KafkaMessageBus.Serializer
{
    /// <summary>
    /// 2.1.152
    /// </summary>
   /*
    public class MessagePackSerializerBak : ISerializer
    {
        private readonly IFormatterResolver _formatterResolver;
        private readonly bool _useCompression;

        private MessagePackSerializerOptions _compressionOptions;
        private MessagePackSerializerOptions _unCompressionOptions;

        public MessagePackSerializer(IFormatterResolver resolver = null, bool useCompression = false)
        {
            _useCompression = useCompression;
            _formatterResolver = resolver ?? ContractlessStandardResolver.Instance;

            _compressionOptions = MessagePackSerializerOptions.Standard.WithResolver(_formatterResolver).WithCompression(MessagePackCompression.Lz4BlockArray);
            _unCompressionOptions = MessagePackSerializerOptions.Standard.WithResolver(_formatterResolver);
        }

        public T Deserialize<T>(byte[] bytes)
        {
            if (_useCompression)
            {
                return MessagePack.MessagePackSerializer.Deserialize<T>(bytes, _compressionOptions);
            }
            else
            {
                return MessagePack.MessagePackSerializer.Deserialize<T>(bytes, _unCompressionOptions);
            }

        }

        public byte[] Serialize<T>(T data)
        {
            if (_useCompression)
            {
                return MessagePack.MessagePackSerializer.Serialize(data, _compressionOptions);
            }
            else
            {
                return MessagePack.MessagePackSerializer.Serialize(data, _unCompressionOptions);
            }
        }
    }

    */

    /// <summary>
    /// 1.7.34
    /// </summary>
    //public class MessagePackSerializerImpl : ISerializer
    //{
    //    private readonly IFormatterResolver _formatterResolver;
    //    private readonly bool _useCompression;

    //    public MessagePackSerializerImpl(IFormatterResolver resolver = null, bool useCompression = false)
    //    {
    //        _useCompression = useCompression;
    //        _formatterResolver = resolver ?? ContractlessStandardResolver.Instance;
    //    }

    //    public T Deserialize<T>(byte[] bytes)
    //    {
    //        if (_useCompression)
    //            return MessagePack.LZ4MessagePackSerializer.Deserialize<T>(bytes, _formatterResolver);
    //        else
    //            return MessagePack.MessagePackSerializer.Deserialize<T>(bytes, _formatterResolver);
    //    }

    //    public byte[] Serialize<T>(T data)
    //    {
    //        if (_useCompression)
    //            return MessagePack.LZ4MessagePackSerializer.Serialize(data, _formatterResolver);
    //        else
    //            return MessagePack.MessagePackSerializer.Serialize(data, _formatterResolver);
    //    }
    //}

    public class MessagePackSerializerDeaultImpl : ISerializer
    {
        public T Deserialize<T>(byte[] bytes)
        {
            return MessagePack.MessagePackSerializer.Deserialize<T>(bytes);
        }

        public byte[] Serialize<T>(T data)
        {
            return MessagePack.MessagePackSerializer.Serialize(data);
        }
    }

    public class SerializerFactory
    {
        public static SerializerFactory Instance = new SerializerFactory();

        private static ISerializer Serializer = new MessagePackSerializerDeaultImpl();
        private SerializerFactory() { }


       public ISerializer GetSerialize()
        {
            return Serializer;
        }
    }
}
