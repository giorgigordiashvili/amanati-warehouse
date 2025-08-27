"use strict";

//https://learn.microsoft.com/en-us/aspnet/core/tutorials/signalr?view=aspnetcore-8.0&tabs=visual-studio

var connection = new signalR.HubConnectionBuilder().withUrl("/CommunicationHub",
    {
        headers: { "CustomUserID": "0" },
        headers: { "CustomUserType": "1" },
    }
).build();


connection.start().then(function () {
    //Connection Established

}).catch(function (err) {
    return console.error(err.toString());
});

connection.on("ReceiveDataFromServerWeb", function (id, parcelCount, message, parcels, fullName, roomNumber, groupName, GroupedParcels) {

    audio.play();

    switch (id) {
        case 1:
            clientSMSCode1 = message;
            break;
        case 2:

            clientSMSCode2 = message;
            break;
        case 3:

            clientSMSCode3 = message;

            break;
        default:
        // code block
    }

    //console.log(GroupedParcelResponse);



    var codeCountDiv = document.getElementById("Name" + id);
    codeCountDiv.innerText = message + " -\t" + parcelCount + " ამანათი";

    var groupDiv = document.getElementById("Group" + id);
    var groupDivText = document.createElement('div');
    groupDivText.className = 'alert-danger btn-xs btn-block text-center';
    groupDivText.innerHTML = groupName;
    groupDiv.appendChild(groupDivText);

    var nameRoomNumberDiv = document.getElementById("Name" + (id + 3));
    nameRoomNumberDiv.innerText = "GE" + roomNumber + " -\t" + fullName;

    var contetntDiv = document.getElementById("Contetnt" + id);
    contetntDiv.innerHTML = '';
    if (contetntDiv) {


        GroupedParcels.forEach(function (GroupedParcel) {
            var groupedparceldiv = document.createElement('div');

            var groupDivText = document.createElement('div');
            groupDivText.className = 'alert-success btn-xs btn-block text-center';
            groupDivText.innerHTML = GroupedParcel.fullName;
            groupedparceldiv.appendChild(groupDivText);

            contetntDiv.appendChild(groupedparceldiv);

            GroupedParcel.items.forEach(function (item) {
                console.log(item);

                var newCheckBox = document.createElement('input');
                newCheckBox.type = 'checkbox';

                newCheckBox.id = item.trackingNumber;
                newCheckBox.value = item.id;
                newCheckBox.checked = false;

                var d = document.createElement('div');
                d.innerText =
                    " " + item.trackingNumber + " -\t" + item.weight + " კგ -\t" + item.flightName + " -\t" + item.shelfName;
                d.className = 'h5 mb-0 text-gray-900';
                d.prepend(newCheckBox);
                contetntDiv.appendChild(d);
            });

            //console.log(GroupedParcel);
        });


        //parcels.forEach(function (parcel) {
        //    //console.log(parcel);

        //    var newCheckBox = document.createElement('input');
        //    newCheckBox.type = 'checkbox';

        //    newCheckBox.id = parcel.trackingNumber;
        //    newCheckBox.value = parcel.id;
        //    newCheckBox.checked = false;
        //    //contetntDiv.appendChild(newCheckBox);

        //    var d = document.createElement('div');
        //    d.innerText =
        //        " " + parcel.trackingNumber + " -\t" + parcel.weight + " კგ -\t" + parcel.flightName + " -\t" + parcel.shelfName;
        //    d.className = 'h5 mb-0 text-gray-900';
        //    d.prepend(newCheckBox);

        //    contetntDiv.appendChild(d);

        //    //var d = document.createElement('div');
        //    //d.innerText = parcel.TrackingNumber;
        //    //d.className = 'h5 mb-0';
        //    //contetntDiv.appendChild(d);
        //});
    }
});



connection.on("GetResponseFromServer", function (clientID, status, message) {

    if (status == 0) {
        Swal.fire({
            title: message,
            icon: "error",
            timer: 2000
        });
    }
    else {
        ClearParcelBox(clientID);
        Swal.fire({
            title: message,
            icon: "success",
            timer: 2000
        });
    }
});


connection.on("WorkstationeStatusUpdate", function (workstationeList) {
    //console.log(workstationeList);

    workstationeList.forEach(function (item) {        
        //console.log(item);        
        var name = document.getElementById("Workstatione" + item.workstationeID);       

        if (item.connected == true) {
            name.classList.remove("btn-danger");
            name.classList.add("btn-success");
        }
        else {
            name.classList.remove("btn-success");
            name.classList.add("btn-danger");
        }
    });
});




function ApproveWithdraw(workstationeNumber, checkedCheckboxValues, clientSMSCode) {
    connection.invoke("SendDataToServerWeb", workstationeNumber, clientSMSCode, checkedCheckboxValues, true).catch(function (err) {
        return console.error(err.toString());
    });
}

function DeclineWithdraw(workstationeNumber) {
    connection.invoke("SendDataToServerWeb", workstationeNumber, null, null, false).catch(function (err) {
        return console.error(err.toString());
    });
}

function FoceClearNotification(workstationeNumber) {
    connection.invoke("FoceClearNotification", workstationeNumber).catch(function (err) {
        return console.error(err.toString());
    });
}




//Disable the send button until connection is established.
//document.getElementById("sendButton").disabled = true;

//connection.on("ReceiveMessage", function (user, message) {
//    var li = document.createElement("li");
//    document.getElementById("messagesList").appendChild(li);
//    // We can assign user-supplied strings to an element's textContent because it
//    // is not interpreted as markup. If you're assigning in any other way, you
//    // should be aware of possible script injection concerns.
//    li.textContent = `${user} says ${message}`;
//});



//document.getElementById("sendButton").addEventListener("click", function (event) {
//    var user = document.getElementById("userInput").value;
//    var message = document.getElementById("messageInput").value;
//    connection.invoke("SendMessage", user, message).catch(function (err) {
//        return console.error(err.toString());
//    });
//    event.preventDefault();
//});