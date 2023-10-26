$(() => {
    LoadProdData();
    var connection = new signalR.HubConnectionBuilder().withUrl("/postSignalR").build();

    connection.start().then(function () {
        console.log("SignalR connection established.");
    }).catch(function (error) {
        console.error("SignalR connection error: " + error);
    });

    connection.on("LoadPosts", function () {
        LoadProdData();
    });

    function LoadProdData() {
        console.log("check")
        var tr = '';
        $.ajax({
            url: '/Posts/GetPosts',
            method: 'GET',
            success: function (result) {
                console.log(result);
                $.each(result, function (k, v) {
                    tr += `<tr>
                        <td> ${v.createdDate}</td> 
                        <td> ${v.updateDate}</td> 
                        <td> ${v.title}</td> 
                        <td> ${v.content}</td> 
                        <td> ${v.publishStatus}</td> 
                        <td> ${v.authorName}</td> 
                        <td> ${v.categoryName}</td> 
                        <td>
                            <a href='../Posts/Edit?id=${v.postId}'>Edit </a> | 
                            <a href='../Posts/Details?id=${v.postId}'>Details</a> | 
                            <a href='../Posts/Delete?id=${v.postId}'>Delete</a>
                        </td> 
                        </tr>`;
                });
                $("#tableBody").html(tr);
            },
            error: function (error) {
                console.log(error);
            }
        });
    }

   
});