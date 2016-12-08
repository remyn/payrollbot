
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
    <div className="portlet-body chat-widget">
        <div className="row">
            <div className="col-lg-12">
                <p className="text-center text-muted small">Wednesday, 7 December 2016</p>
            </div>
        </div>
{messageNodes}
</div>
</div>
    );
}
});

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
        this.setState({author: '', text: ''});
    },
    render: function() {
        return (		
            <div className="portlet-footer">
                <form role="form" onSubmit={this.handleSubmit}>
                    <div className="form-group">
                        <textarea className="form-control" placeholder="Start typing..." value= { this.state.text } onChange= { this.handleTextChange }></textarea>
                    </div>
                    <div className="form-group">                     
                        <button type="submit" className="btn btn-default pull-right" value= { this.state.text } onChange= { this.handleTextChange }>Send</button>
                        <div className="clearfix"></div>
                    </div>
                </form>
            </div>
      );
    }
});


var ChatBox = React.createClass({
    loadCommentsFromServer: function() {
        var xhr = new XMLHttpRequest();
        xhr.open('get', this.props.url, true);
        xhr.onload = function() {
            var data = JSON.parse(xhr.responseText);
            this.setState({ data: data });
        }.bind(this);
        xhr.send();
    },
    handleCommentSubmit: function(message) {
        var messages = this.state.data;
        message.id = Date.now();
        var newMessages = messages.concat([message]);
        this.setState({data: newMessages});

        var data = new FormData();
        data.append('Author', message.Author);
        data.append('Text', message.Text);

        var xhr = new XMLHttpRequest();
        xhr.open('post', this.props.submitUrl, true);
        xhr.onload = function() {
            this.handleBotAnswer(message.Text);
            this.loadCommentsFromServer();
        }.bind(this);
        xhr.send(data);
    },
    handleBotAnswer: function(message) {
        var params = "message=" + message;
        var xhr = new XMLHttpRequest();
        xhr.open('post', this.props.addBotAnswerUrl, true);
        xhr.setRequestHeader('Content-type', 'application/x-www-form-urlencoded');
        xhr.send(params);
    },
    getInitialState: function() {
        return {data: []};
    },
    componentDidMount: function() {
        this.loadCommentsFromServer();
        window.setInterval(this.loadCommentsFromServer, this.props.pollInterval);
    },
    render: function() {
        return (
		<div className="chatBox container bootstrap snippet">
			<div className="row">
				<div className="portlet portlet-default">
					<div className="portlet-heading">
						<div className="portlet-title">
							<h4><i className="fa fa-circle text-green"></i> Chat</h4>
						</div>
						<div className="portlet-widgets">
							<span className="divider"></span>
							<a data-toggle="collapse" data-parent="#accordion" href="#chat"><i className="fa fa-chevron-down"></i></a>
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
  <ChatBox url="/comments" submitUrl="/comments/new" addBotAnswerUrl="/comments/addBotAnswer" pollInterval={2000} />,
  document.getElementById('content')
);
