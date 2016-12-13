var Notification = React.createClass({
    render: function () {
        var status = this.props.status;
        return (
            <tr data-status={status} className={(this.props.isSeen ? "" : "selected")}>
                <td>
                    <div className="ckbox">
                        <input type="checkbox" id="checkbox1" />
                        <label htmlFor="checkbox1"></label>
                    </div>
                </td>
                <td>
                    <a href="javascript:;" className={"star " + (this.props.isStarred ? 'star-checked' : '')}>
                        <i className="glyphicon glyphicon-star"></i>
                    </a>
                </td>
                <td>
                    <div className="media">
                        <a href="#" className="pull-left">
                                <img src="https://s3.amazonaws.com/uifaces/faces/twitter/fffabs/128.jpg" className="media-photo" />
                        </a>
                        <div className="media-body">
                            <span className="media-meta pull-right">{this.props.notificationTime}</span>
                            <h4 className="title">
                                {this.props.title}
                                <span className={"pull-right " + status}>{this.props.status}</span>
                            </h4>
                            <p className="summary">{this.props.children}</p>
                        </div>
                    </div>
                </td>
            </tr>
    );
    }
});


var NotificationList = React.createClass({
    render: function() {
        var notificationNodes = this.props.data.map(function(notification) {
            return (
              <Notification status={notification.Status} isSeen={notification.IsSeen} isStarred={notification.IsStarred} 
                       notificationTime={notification.NotificationTime} title={notification.Title} key={notification.Id}>
                {notification.Text}
              </Notification>
            );
        });
        return (
            <table className="table table-filter">
                <tbody>
                    {notificationNodes}
                </tbody>
            </table>
        );
}});


var NotificationBox = React.createClass({
    loadCommentsFromServer: function () {
        if (this.isMounted()) {
            var xhr = new XMLHttpRequest();
            xhr.open('get', this.props.url, true);
            xhr.onload = function () {
                var data = JSON.parse(xhr.responseText);
                this.setState({
                    notifications: data.Notifications,
                    numberOfNewNotifications: data.NumberOfNewNotifications
                });
            }.bind(this);
            xhr.send();

            this.setNumberOfNewNotifications(this.state.numberOfNewNotifications);
        }
    },
    getInitialState: function() {
        return {
            notifications: [],
            numberOfNewNotifications: 0
        };
    },
    componentDidMount: function () {
        this.loadCommentsFromServer();
        window.setInterval(this.loadCommentsFromServer, this.props.interval);
    },
    setNumberOfNewNotifications: function (numberOfNewNotifications) {
        $("#notificationBadge").text(numberOfNewNotifications);
    },
    render: function() {
        return (
		    <div className="hidden" id="a1">
                <div className="popover-body">
                    <div className="row">
                        <section className="content">
                            <div className="panel-body">
                                <div className="pull-right">
                                    <div className="btn-group">
                                        <button type="button" className="btn btn-success btn-filter" data-target="payroll">Payroll</button>
                                        <button type="button" className="btn btn-warning btn-filter" data-target="core">Core</button>
                                        <button type="button" className="btn btn-danger btn-filter" data-target="system">System</button>
                                        <button type="button" className="btn btn-default btn-filter" data-target="all">All</button>
                                    </div>
                                </div>
                                <br />
                                <br />
                                <br />
                                <div className="table-container">
                                    <NotificationList data={this.state.notifications} />
                                </div>
                            </div>
                        </section>
                    </div>
                </div>
            </div>
        );
    }
});

ReactDOM.render(
  <NotificationBox url="/notifications" interval={2000} />,
  document.getElementById('notificationsContent')
);
