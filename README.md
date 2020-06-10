# Aix.KafkaMessageBus
#### kafka订阅发布
#### 注入 ConfigureServices方法中

```
  var kafkaMessageBusOptions = context.Configuration.GetSection("kafka").Get<KafkaMessageBusOptions>();
  services.AddKafkaMessageBus(kafkaMessageBusOptions);
```

#### 注入 IMessageBus _messageBus，添加业务实体
```
   [TopicAttribute(Name = "BusinessMessage")]
    public class BusinessMessage
    {
        [RouteKeyAttribute]
        public string MessageId { get; set; }
        public string Content { get; set; }
        public DateTime CreateTime { get; set; }
    }
```


#### 生产消息
```  
   var messageData = new BusinessMessage
     {
	   MessageId = "1",
	   Content = $"我是内容",
	   CreateTime = DateTime.Now
      };
      await _messageBus.PublishAsync(messageData);

```

#### 消费消息
``` 
     await _messageBus.SubscribeAsync<BusinessMessage>(async (message) =>
	  {
	    var current = Interlocked.Increment(ref Count);
	    _logger.LogInformation($"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff")}消费--1--数据：MessageId={message.MessageId},Content= {message.Content},count={current}");
	    await Task.CompletedTask;
	 });

   //或者
   //订阅配置可以灵活的增加参数 支持参数如下
	SubscribeOptions subscribeOptions = new SubscribeOptions();
	subscribeOptions.GroupId = "group2";
	subscribeOptions.ConsumerThreadCount = 2;
       
  await _messageBus.SubscribeAsync<BusinessMessage>(async (message) =>
	{
	    var current = Interlocked.Increment(ref Count);
	    _logger.LogInformation($"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff")}消费--2--数据：MessageId={message.MessageId},Content={message.Content},count={current}");
	    await Task.CompletedTask;
	}, subscribeOptions, cancellationToken);

```




