
var Message = React.createClass({
    render: function () {
        var isBot = this.props.isBot;
        var path;
        if (isBot)
            path = "/Content/reckon_logo.png";
        else
            path = "/Content/user.png";
        
        return (
            <div className="row">
                <div className="col-lg-12">
                    <div className="media">
                        <a className="pull-left" href="#">
                            <img className="media-object" src={path} />
                        </a>
                        <div className="media-body">
                            <h4 className="messageAuthor media-heading">
                                {this.props.author}
                                <span className="small pull-right">{this.props.messageTime}</span>
                            </h4>
                            <p>{this.props.children}</p>
                        </div>
                    </div>
                </div>
            </div>
    );
    }
});

var MessageList = React.createClass({
    render: function() {
        var messageNodes = this.props.data.map(function(message) {
            return (
              <Message author={message.Author} isBot={message.IsBot} messageTime={message.MessageTime} key={message.Id}>
          {message.Text}
        </Message>
      );
    });
    return (
      <div className="messageList ">
        <div id="scrollable-div" className="portlet-body chat-widget scrollable-window">
            <div className="row">
                <div className="col-lg-12">
                    <p className="text-center text-muted small">{$("#currentDate").val()}</p>
                </div>
            </div>
            {messageNodes}
        </div>
    </div>
    );
}});

var MessageForm = React.createClass({
    getInitialState: function() {
        return {author: '', text: ''};
    },
    handleTextChange: function(e) {
        this.setState({text: e.target.value});
    },
    handleSubmit: function(e) {
        e.preventDefault();
        var author = "Reckon User";
        var text = this.state.text.trim();
        if (!text) {
            return;
        }
        this.props.onCommentSubmit({Author: author, Text: text});
        this.setState({ author: '', text: '' });
    },
    render: function() {
        return (		
            <div className="portlet-footer">
                <form role="form" onSubmit={this.handleSubmit}>
                    <div className="input-group input-group-unstyled">
                         <input type="text" className="form-control" placeholder="Ask something..." value={ this.state.text } onChange={ this.handleTextChange } />
                         <span className="input-group-addon">
                            <a data-toggle="tooltip" data-placement="bottom" title="Activate Speech to Text"><i className="fa fa-microphone fa-2x" aria-hidden="true"></i></a>
                         </span>
                        <div className="input-group-btn">
                            <button type="submit" className="pull-right reckonbutton" value={ this.state.text } onChange={ this.handleTextChange }>Send</button>
                            <div className="clearfix"></div>
                        </div>
                    </div>
                </form>
            </div>
      );
    }
});


var ChatBox = React.createClass({
    scrollToBottom: function(){
        $("#scrollable-div").scrollTop($("#scrollable-div")[0].scrollHeight);
    },
    attachTooltip: function(){
        $('[data-toggle="tooltip"]').tooltip();
    },
    loadCommentsFromServer: function () {
        this.scrollToBottom();
        var xhr = new XMLHttpRequest();
        xhr.open('get', this.props.url, true);
        xhr.onload = function() {
            var data = JSON.parse(xhr.responseText);
            this.setState({ data: data });
            this.scrollToBottom();
        }.bind(this);
        xhr.send();
        this.scrollToBottom();
    },
    handleCommentSubmit: function(message) {
        var messages = this.state.data;
        message.Id = Date.now();
        message.IsBot = false;
        var newMessages = messages.concat([message]);
        this.setState({ data: newMessages });
        this.scrollToBottom();

        var data = new FormData();
        data.append('Author', message.Author);
        data.append('Text', message.Text);

        var xhr = new XMLHttpRequest();
        xhr.open('post', this.props.submitUrl, true);
        xhr.onload = function () {
            var botAnswer = JSON.parse(xhr.responseText);
            this.playTextAsSpeech(botAnswer);
            this.loadCommentsFromServer();
        }.bind(this);
        xhr.send(data);
    },
    playTextAsSpeech: function (message) {
        var volume = $("#volume");
        if (!volume.hasClass("active")) return;

        $.ajax({
            url: "/Home/ConvertTextToSpeech",
            type: "POST",
            data: JSON.stringify({ text: message }),
            dataType: "json",
            async: true,
            contentType: 'application/json; charset=utf-8',
            success: function (filePath) {
                var audio = document.getElementById("audioControl");
                audio.src = filePath;
                console.log(filePath);
                audio.load();
                audio.play();
        },
        error: function (xhr, status) {
            var err = "Error " + " " + status;
            if (xhr.responseText && xhr.responseText[0] === "{")
                err = JSON.parse(xhr.responseText).Message;
            console.log(err);
        }});
    },
    getInitialState: function() {
        return {data: []};
    },
    componentWillUpdate: function(){
        this.scrollToBottom();
    },
    componentDidMount: function () {
        this.scrollToBottom();
        this.attachTooltip();
        this.loadCommentsFromServer();
        window.setInterval(this.loadCommentsFromServer, this.props.pollInterval);
    },
    toggleListening: function(e){
        var volume = $("#volume");
        
        if(volume.hasClass("active"))
        {
            volume.removeClass("active");
            volume.addClass("disabled");
            volume.find($(".fa")).removeClass('fa-volume-up').addClass('fa-volume-off');
        }
        else
        {
            volume.removeClass("disabled");
            volume.addClass("active");
            volume.find($(".fa")).removeClass('fa-volume-off').addClass('fa-volume-up');
        }

    },
    render: function() {
        return (
		<div className="chatBox container bootstrap snippet">
            <div className="row">
				<div className="portlet portlet-default">
					<div className="portlet-heading">
						<div className="row">

                            <div className="col-md-10">
                                <div className="input-group input-group-unstyled">
                                     <h4 className="col-md-6">
                                        <i className="fa fa-circle text-green"></i> Chat
                                     </h4>
                                    <span className="input-group-addon" onClick={ this.toggleListening }>
                                        <a id="volume" data-toggle="tooltip" data-placement="top" title="Text to Speech activated" className="btn btn-small active display-flex">
                                                <i className="fa fa-volume-up"></i>
                                        </a>
                                    </span>
                                </div>
                            </div>
                            <div className="col-md-2 portlet-widgets">
							    <span className="divider"></span>
							    <a data-toggle="collapse" data-parent="#accordion" href="#chat"><i className="fa fa-chevron-down"></i></a>
                            </div>
                        </div>
						<div className="clearfix"></div>
					</div>

					<div id="chat" className="panel-collapse collapse in">
						<MessageList data={this.state.data} />

						<MessageForm onCommentSubmit={this.handleCommentSubmit} />
					</div>
					<br />
				</div>
			</div>	
		</div>
    );
    }
});

ReactDOM.render(
  <ChatBox url="/comments" submitUrl="/comments/new" pollInterval={2000} />,
  document.getElementById('content')
);
