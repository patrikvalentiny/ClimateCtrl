﻿using api.ClientEventHandlers;
using api.Utils;
using Fleck;
using FluentAssertions;
using MediatR;
using Moq;

namespace UnitTests;

public class WsHelperTests
{
    [Test]
    public async Task InvokeBaseDtoHandlerTest()
    {
        var socketMock = new Mock<IWebSocketConnection>();
        var socket = socketMock.Object;
        const string dtoString = """{"eventType":"ClientSaysHello","message":"message"}""";
        var mediatr = new Mock<IMediator>();
        mediatr.Setup(x => x.Send(It.IsAny<BaseDto>(), default))
            .ReturnsAsync(new ServerSaysHelloDto { Message = "Hello, message!" });
        var act = async () => await socket.InvokeBaseDtoHandler(dtoString, mediatr.Object);
        await act.Should().NotThrowAsync();
    }

    [Test]
    public async Task InvokeBaseDtoHandlerTestWithInvalidDto()
    {
        var socketMock = new Mock<IWebSocketConnection>();
        var socket = socketMock.Object;
        const string dtoString = """{"eventType":"Invalid","message":"message"}""";
        var mediatr = new Mock<IMediator>();
        mediatr.Setup(x => x.Send(It.IsAny<BaseDto>(), default))
            .ReturnsAsync(new ServerSaysHelloDto { Message = "Hello, message!" });
        var act = async () => await socket.InvokeBaseDtoHandler(dtoString, mediatr.Object);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Test]
    public async Task ServerSendsNotificationTest()
    {
        var socketMock = new Mock<IWebSocketConnection>();
        var socket = socketMock.Object;
        var act = async () => await socket.SendNotification("message");
        await act.Should().NotThrowAsync();
        act = async () => await socket.SendSuccess("message");
        await act.Should().NotThrowAsync();
        act = async () => await socket.SendError("message");
        await act.Should().NotThrowAsync();
        act = async () => await socket.SendWarning("message");
        await act.Should().NotThrowAsync();
    }

    [Test]
    public async Task HandleExceptionTest()
    {
        var socketMock = new Mock<IWebSocketConnection>();
        var socket = socketMock.Object;
        var ex = new WsHelper.HandlerException("message");
        var act = async () => await ex.Handle(socket);
        await act.Should().NotThrowAsync();
    }


    // [Test]
    // public async Task ClientControlsMotorTest()
    // {
    //     var socketMock = new Mock<IWebSocketConnection>();
    //     var socket = socketMock.Object;
    //     const string dtoString = """{"eventType":"ClientControlsMotor","mac":"mac","position":1}""";
    //     var mediatr = new Mock<IMediator>();
    //     mediatr.Setup(x => x.Send(It.IsAny<BaseDto>(), default)).ReturnsAsync(new ServerSendsMotorDataDto { Mac = "mac", Position = 1 });
    //     await socket.InvokeBaseDtoHandler(dtoString, mediatr.Object);
    // }
    //
    // [Test]
    // public async Task ClientStartsListeningToMotorTest()
    // {
    //     var socketMock = new Mock<IWebSocketConnection>();
    //     var socket = socketMock.Object;
    //     const string dtoString = """{"eventType":"ClientStartsListeningToMotor","mac":"mac"}""";
    //     var mediatr = new Mock<IMediator>();
    //     mediatr.Setup(x => x.Send(It.IsAny<BaseDto>(), default)).ReturnsAsync(new ClientStartsListeningToMotorDto { Mac = "mac" });
    //     await socket.InvokeBaseDtoHandler(dtoString, mediatr.Object);
    // }
}