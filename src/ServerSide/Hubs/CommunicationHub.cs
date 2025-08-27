using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using ServerSide.Enums;
using ServerSide.Infrastructure.ParcelService;
using ServerSide.Models;
using ServerSide.Models.ParcelService;
using ServerSide.OptionsModels;

namespace ServerSide.Hubs
{
    public class CommunicationHub : Hub
    {
        private IParcelRestClient _parcelRestClient;

        private readonly ILogger<CommunicationHub> _logger;

        public static Dictionary<int, LanguageEnum> _LanguageData = new();

        private readonly IHttpContextAccessor _httpContextAccessor;

        public static List<ConnectedUser> ConnectedUsers = new();

        public static List<ExchangeData> ExchangeDataList = new();

        private readonly ParcelOptions _parcelOptions;

        public static class UserHandler
        {
            public static readonly HashSet<string> ConnectedIds = new();
            public static int Ids = new();
        }


        public CommunicationHub(
            IParcelRestClient parcelRestClient,
            ILogger<CommunicationHub> logger,
            IHttpContextAccessor httpContextAccessor,
            IOptions<ParcelOptions> parcelOptions)
        {
            _parcelRestClient = parcelRestClient;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
            _parcelOptions = parcelOptions.Value;
        }


        #region SignalR


        #region Send Data

        //(from Client App)
        public async Task SendDataToServerApp(SendDataModel model)
        {
            var clientLanguage = _LanguageData.Where(x => x.Key == model.ClientID);

            if (!clientLanguage.Any())
                _LanguageData.Add(model.ClientID, model.Language);
            else
                _LanguageData[model.ClientID] = model.Language;

            try
            {
                var coderesponse = await _parcelRestClient.CheckUserCode(model.Code);

                var clientMessage = new ReciveDataModel();

                switch (coderesponse.Code)
                {
                    //Error Check Code                        
                    case "0":
                        clientMessage = new ReciveDataModel()
                        {
                            ClientID = model.ClientID,
                            Message = model.Language == LanguageEnum.GE ? "დაფიქსირდა შეცდომა გადამოწმებისას!" : "EN დაფიქსირდა შეცდომა გადამოწმებისას!",
                            StatusID = (int)MessageStyleEnum.notoficationWarning
                        };
                        //Send Data To Client
                        await Clients.All.SendAsync("ReceiveDataFromServerApp", clientMessage);
                        break;

                    case "100":
                        clientMessage = new ReciveDataModel()
                        {
                            ClientID = model.ClientID,
                            Message = coderesponse.Message,
                            StatusID = (int)MessageStyleEnum.notoficationWait
                        };

                        //Send Data To Client
                        await Clients.All.SendAsync("ReceiveDataFromServerApp", clientMessage);


                        //Send Data To Server

                        var personData = coderesponse.Items.FirstOrDefault();
                        var fullName = personData != null ? personData.FullName != "" ? personData.FullName : personData.OrganizationName : "";
                        var roomNumber = personData?.Code ?? 0;
                        var groupName = personData?.UserGroupName ?? "";


                        var uniquePersonalNumbers = coderesponse.Items.Select(x => x.PersonalNumber).ToList().Distinct().ToList();
                        foreach (var uniquePersonalNumber in uniquePersonalNumbers)
                        {
                            var uniquePerson = coderesponse.Items.Where(x => x.PersonalNumber == uniquePersonalNumber).FirstOrDefault();
                            if (uniquePerson != null)
                            {
                                var uniquePersonItems = coderesponse.Items.Where(x => x.PersonalNumber == uniquePersonalNumber).ToList();

                                var groupedParcel = new GroupedParcel()
                                {
                                    PersonalNumber = uniquePerson.PersonalNumber,
                                    FullName = uniquePerson.FullName,
                                    Items = uniquePersonItems
                                };
                                coderesponse.GroupedParcels.Add(groupedParcel);
                            }
                        }



                        await Clients.All.SendAsync("ReceiveDataFromServerWeb",
                            model.ClientID,
                            coderesponse.Items.Count,
                            coderesponse.ItemCode,
                            coderesponse.Items,
                            fullName,
                            roomNumber,
                            groupName,
                            coderesponse.GroupedParcels
                            );

                        //Save Temporary Data
                        ExchangeDataList.Add(new ExchangeData()
                        {
                            ClientID = model.ClientID,
                            Count = coderesponse.Items.Count,
                            ItemCode = coderesponse.ItemCode,
                            Items = coderesponse.Items,
                            FullName = fullName,
                            RoomNumber = roomNumber,
                            GroupName = groupName,
                            GroupedParcels = coderesponse.GroupedParcels
                        });


                        break;

                    case "-100":
                    case "-200":
                    case "-300":
                    case "-400":
                    case "-500":
                    case "-600":
                    case "-700":
                    case "-800":
                    case "-900":
                        clientMessage = new ReciveDataModel()
                        {
                            ClientID = model.ClientID,
                            Message = coderesponse.Message,
                            StatusID = (int)MessageStyleEnum.notoficationWarning
                        };
                        //Send Data To Client
                        await Clients.All.SendAsync("ReceiveDataFromServerApp", clientMessage);

                        break;

                    default:
                        clientMessage = new ReciveDataModel()
                        {
                            ClientID = model.ClientID,
                            Message = "შეცდომა კოდის გადამოწმებისას, მიმართეთ ოპერატორს",
                            StatusID = (int)MessageStyleEnum.notoficationWarning
                        };
                        //Send Data To Client
                        await Clients.All.SendAsync("ReceiveDataFromServerApp", clientMessage);

                        break;
                }

            }
            catch (Exception ex)
            {
                var error = ex.Message ?? "Error In SendDataToServerApp";
                _logger.LogError(error);

                var clientMessage = new ReciveDataModel()
                {
                    ClientID = model.ClientID,
                    Message = model.Language == LanguageEnum.EN ? "EN დაფიქსირდა შეცდომა გადამოწმებისას!" : "დაფიქსირდა შეცდომა გადამოწმებისას!",
                    StatusID = (int)MessageStyleEnum.notoficationWarning
                };
                //Send Data To Client
                await Clients.All.SendAsync("ReceiveDataFromServerApp", clientMessage);
            }
        }

        //(from Server Web)
        public async Task SendDataToServerWeb(int workstationNumber, string smsCode, List<string> parcels, bool isWithdrawal)
        {
            var requstLanguageEnum = LanguageEnum.GE;

            var clientLastRequestLanguageData = _LanguageData.Where(x => x.Key == workstationNumber);
            if (clientLastRequestLanguageData.Any())
                requstLanguageEnum = clientLastRequestLanguageData.FirstOrDefault().Value;


            if (isWithdrawal)
            {
                try
                {
                    //Call Withdraw API
                    var withdrawResponse = await _parcelRestClient.WithdrawParcels(smsCode, parcels);

                    if (withdrawResponse.Code == 100)
                    {
                        //Send Client Take Parcels Notification                        
                        var clientMessage = new ReciveDataModel()
                        {
                            ClientID = workstationNumber,
                            Message = requstLanguageEnum == LanguageEnum.GE ? "გთხოვთ აიღოთ ამანათები" : "EN გთხოვთ აიღოთ ამანათები",
                            StatusID = (int)MessageStyleEnum.notoficationTake
                        };

                        await Clients.All.SendAsync("ReceiveDataFromServerApp", clientMessage);

                        //ToDo
                        //Send Server 
                        await SendResponseToWeb(workstationNumber, 1, "ამანათის გაცემა დასრულებულია");


                        //Remove From Temporary
                        ExchangeDataList.RemoveAll(X => X.ClientID == workstationNumber);

                    }
                    else
                    {
                        _logger.LogError($"Can't withdraw Parcels, Code: {smsCode}");

                        //Send Client Error Notification                        
                        var clientMessage = new ReciveDataModel()
                        {
                            ClientID = workstationNumber,
                            Message = requstLanguageEnum == LanguageEnum.GE ? "შეცდომა ამანათის გაცემისას, მიმართეთ ოპერატორს" : "EN შეცდომა ამანათის გაცემისას, მიმართეთ ოპერატორს",
                            StatusID = (int)MessageStyleEnum.notoficationWarning
                        };

                        await Clients.All.SendAsync("ReceiveDataFromServerApp", clientMessage);

                        //ToDo
                        //Send Server 
                        await SendResponseToWeb(workstationNumber, 0, "შეცდომა ამანათის გაცემისას");
                    }
                }
                catch (Exception ex)
                {
                    var error = ex.Message ?? "Error In SendDataToServerWeb";
                    _logger.LogError(error);

                    //Send Client Error Notification                        
                    var clientMessage = new ReciveDataModel()
                    {
                        ClientID = workstationNumber,
                        Message = requstLanguageEnum == LanguageEnum.GE ? "შეცდომა ამანათის გაცემისას, მიმართეთ ოპერატორს" : "EN შეცდომა ამანათის გაცემისას, მიმართეთ ოპერატორს",
                        StatusID = (int)MessageStyleEnum.notoficationWarning
                    };

                    await Clients.All.SendAsync("ReceiveDataFromServerApp", clientMessage);

                    //ToDo
                    //Send Server 
                    await SendResponseToWeb(workstationNumber, 0, "შეცდომა ამანათის გაცემისას");
                }
            }
            else
            {
                var clientMessage = new ReciveDataModel()
                {
                    ClientID = workstationNumber,
                    Message = requstLanguageEnum == LanguageEnum.GE ? "ამანათების გაცემა გაუქმებულია!" : "EN ამანათების გაცემა გაუქმებულია!",
                    StatusID = (int)MessageStyleEnum.notoficationWarning
                };
                //Send Data To Client
                await Clients.All.SendAsync("ReceiveDataFromServerApp", clientMessage);

                //Send Server ok
                await SendResponseToWeb(workstationNumber, 1, "ამანათის გაცემა გაუქმებულია");


                //Remove From Temporary
                ExchangeDataList.RemoveAll(X => X.ClientID == workstationNumber);
            }

        }

        #endregion

        #region Receive Data

        //From Client (app)
        public async Task SendDataToClientApp(ReciveDataModel model)
        {
            await Clients.All.SendAsync("SendDataServer", model);
        }

        //From Server (Web)
        public async Task SendDataToClientWeb(SendDataModel model)
        {
            await Clients.All.SendAsync("ReceiveDataFromClient", model);
        }

        #endregion



        public async Task SendResponseToWeb(int clientID, int status, string message)
        {
            await Clients.All.SendAsync("GetResponseFromServer", clientID, status, message);
        }

        public async Task FoceClearNotification(int clientID)
        {
            await Clients.All.SendAsync("FoceClearNotificationApp", clientID);
        }


        public async Task CheckUnfinishedParcels()
        {
            foreach (var item in ExchangeDataList)
            {
                await Clients.All.SendAsync("ReceiveDataFromServerWeb",
                           item.ClientID,
                           item.Items.Count,
                           item.ItemCode,
                           item.Items,
                           item.FullName,
                           item.RoomNumber,
                           item.GroupName,
                           item.GroupedParcels
                           );
            }
        }


        public override async Task OnConnectedAsync()
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext != null)
            {
                var customUserID = httpContext.Request.Headers["CustomUserID"];
                var customUserType = httpContext.Request.Headers["CustomUserType"];

                var connectedUser = new ConnectedUser()
                {
                    ConnectionID = Context.ConnectionId,
                    CustomUserID = customUserID,
                    UserType = string.IsNullOrEmpty(customUserType) ? "1" : customUserType,
                };

                ConnectedUsers.Add(connectedUser);

                if (connectedUser.UserType == "1")
                    await CheckUnfinishedParcels();

                await ConnectionNotification(connectedUser, true);

            }

            //await Groups.AddToGroupAsync(Context.ConnectionId, "SignalR Users");
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            //Ondisconect
            var currentUser = ConnectedUsers.FirstOrDefault(x => x.ConnectionID == Context.ConnectionId);
            if (currentUser != null)
            {
                ConnectedUsers.Remove(currentUser);
                await ConnectionNotification(currentUser, false);
            }

            await base.OnDisconnectedAsync(exception);
        }


        public async Task ConnectionNotification(ConnectedUser connectedUser, bool connectionType)
        {
            var workstationeConnectionStatusList = new List<WorkstationeConnectedModel>();
            var mustClientCount = _parcelOptions.WorkstationCount;

            switch (connectedUser.UserType)
            {

                case "1"://Server
                    if (connectionType) //Connect
                    {
                        //Check Clients Connected                        
                        for (int i = 1; i <= mustClientCount; i++)
                        {
                            var cUser = ConnectedUsers.Where(x => x.CustomUserID == i.ToString()).FirstOrDefault();
                            if (cUser != null)                            
                                workstationeConnectionStatusList.Add(new WorkstationeConnectedModel() { WorkstationeID = i.ToString(), Connected = true });                            
                            else                            
                                workstationeConnectionStatusList.Add(new WorkstationeConnectedModel() { WorkstationeID = i.ToString(), Connected = false });
                        }
                        
                        await Clients.All.SendAsync("WorkstationeStatusUpdate", workstationeConnectionStatusList);
                        await Clients.All.SendAsync("ServerStatusUpdate", true);
                    }
                    else //Disconect
                    {
                        //SendDataToClients 

                        await Clients.All.SendAsync("ServerStatusUpdate", false);
                    }

                    break;


                case "2": //Client
                    
                    for (int i = 1; i <= mustClientCount; i++)
                    {
                        var cUser = ConnectedUsers.Where(x => x.CustomUserID == i.ToString()).FirstOrDefault();
                        if (cUser != null)
                            workstationeConnectionStatusList.Add(new WorkstationeConnectedModel() { WorkstationeID = i.ToString(), Connected = true });
                        else
                            workstationeConnectionStatusList.Add(new WorkstationeConnectedModel() { WorkstationeID = i.ToString(), Connected = false });
                    }
                    //SendToServer
                    await Clients.All.SendAsync("WorkstationeStatusUpdate", workstationeConnectionStatusList);

                    if (connectionType)//Connect
                    {
                        //Check Server Connected

                        var server = ConnectedUsers.Where(x => x.UserType == "1").ToList();
                        if (server.Count > 0)                        
                            await Clients.All.SendAsync("ServerStatusUpdate", true);
                        else
                            await Clients.All.SendAsync("ServerStatusUpdate", false);

                    }
                    else//Disconect
                    {
                        //Check Clients Connected                        
                        for (int i = 1; i <= mustClientCount; i++)
                        {
                            var cUser = ConnectedUsers.Where(x => x.CustomUserID == i.ToString()).FirstOrDefault();
                            if (cUser != null)
                                workstationeConnectionStatusList.Add(new WorkstationeConnectedModel() { WorkstationeID = i.ToString(), Connected = true });
                            else
                                workstationeConnectionStatusList.Add(new WorkstationeConnectedModel() { WorkstationeID = i.ToString(), Connected = false });
                        }

                        await Clients.All.SendAsync("WorkstationeStatusUpdate", workstationeConnectionStatusList);
                    }



                    break;

                default:
                    break;
            }

        }

        #endregion



    }
}
