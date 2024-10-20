namespace Willow.ServiceHealthAggregator.Snowflake.Tests;

using TechTalk.SpecFlow;
using Xunit.Abstractions;

[Binding]
public class TestNotificationExtensionsStepDefinitions(ITestOutputHelper output)
{
    private Notification? notification;
    private string? result;

    private static readonly DateTimeOffset EnqueuedTime = new(new DateTime(2024, 05, 13));
    private const string Data = "{\"version\":\"1.0\",\"messageId\":\"312e50b2-8a2b-4eeb-8671-c6ba390404cd\",\"messageType\":\"USER_TASK_FAILED\",\"timestamp\":\"2024-05-12T01:28:03.149Z\",\"accountName\":\"EE96414\",\"taskName\":\"PRD_DB.TRANSFORMED.SUSTAINABILITY_CREATE_TABLE_UTILITY_BILLS_TRIGGER_TK\",\"taskId\":\"01b42d69-9282-711c-0000-0000000005fe\",\"rootTaskName\":\"PRD_DB.TRANSFORMED.SUSTAINABILITY_CREATE_TABLE_UTILITY_BILLS_TRIGGER_TK\",\"rootTaskId\":\"01b42d69-9282-711c-0000-0000000005fe\",\"messages\":[{\"runId\":\"2024-05-12T01:08:00Z\",\"scheduledTime\":\"2024-05-12T01:08:00Z\",\"queryStartTime\":\"2024-05-12T01:08:02.074Z\",\"completedTime\":\"2024-05-12T01:28:03.114Z\",\"queryId\":\"01b444a4-0001-3d37-0000-3279032a9806\",\"errorCode\":\"000630\",\"errorMessage\":\"Statement reached its statement or warehouse timeout of 1,200 second(s) and was canceled.\"}]}";
    private const string Instance = "tst-aue-01-wil-ts";

    [Given(@"I have a Notification")]
    public void GivenIHaveANotification()
    {
        notification = new()
        {
            EnqueuedTime = EnqueuedTime,
            Id = "123",
            Subject = "Test Notification",
            Data = Data,
        };
    }

    [When(@"I call ToTeamsMessageString")]
    public void WhenICallToTeamsMessageString()
    {
        Assert.NotNull(notification);
        result = notification.ToTeamsMessageString(Instance);
    }

    [Then(@"the message is formatted correctly for Teams")]
    public void ThenTheMessageIsFormattedCorrectlyForTeams()
    {
        string expected =
        $@"
        {{
            ""@type"": ""MessageCard"",
            ""@context"": ""https://schema.org/extensions"",
            ""summary"": ""Error Notification"",
            ""themeColor"": ""d70000"",
            ""title"": ""Error Notification"",
            ""sections"": [
                {{
                    ""facts"": [
  {{
    ""name"": ""Source"",
    ""value"": ""Snowflake""
  }},
  {{
    ""name"": ""Enqueued Time"",
    ""value"": ""{EnqueuedTime.ToString("O").Replace("+", @"\u002B")}""
  }},
  {{
    ""name"": ""Account Name"",
    ""value"": ""EE96414""
  }},
  {{
    ""name"": ""Task/Pipe Name"",
    ""value"": ""PRD_DB.TRANSFORMED.SUSTAINABILITY_CREATE_TABLE_UTILITY_BILLS_TRIGGER_TK""
  }},
  {{
    ""name"": ""Instance"",
    ""value"": ""{Instance}""
  }}
]
                }},
                {{
                    ""type"": ""TextBlock"",
                    ""text"": ""{Data.Replace("\"", "\\\"")}"",
                    ""wrap"": true
                }}
            ]
        }}";

        Assert.Equal(expected, result);
    }

    [When(@"I call ToEmailBodyString")]
    public void WhenICallToEmailBodyString()
    {
        Assert.NotNull(notification);
        result = notification.ToEmailBodyString(Instance);
    }

    [Then(@"the message is formatted correctly for Email")]
    public void ThenTheMessageIsFormattedCorrectlyForEmail()
    {
        output.WriteLine(result);

        string expected = $@"Notification Details:<br/>
Source: Snowflake<br/>
Enqueued Time: {EnqueuedTime:O}<br/>
Account Name: EE96414<br/>
Task/Pipe Name: PRD_DB.TRANSFORMED.SUSTAINABILITY_CREATE_TABLE_UTILITY_BILLS_TRIGGER_TK<br/>
Instance: {Instance}<br/>
<br/>
Details:<br/>
<pre>{Data}</pre>
";

        Assert.Equal(expected, result);
    }
}
