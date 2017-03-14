using CarPoolTool.Models;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Web;
using System.Web.Hosting;

namespace CarPoolTool.Helpers
{
    public class CalendarHelper
    {
        static private CalendarService service;

        private static void Init()
        {
            if(service == null)
            {
                string[] scopes = { CalendarService.Scope.Calendar, CalendarService.Scope.CalendarReadonly };
                string ApplicationName = "CarPoolTool";
                string credPath = HostingEnvironment.MapPath("~/App_Data/carpooltool-gcalendar-secret.json");

                GoogleCredential credential = GoogleCredential.FromStream(new FileStream(credPath, FileMode.Open)).CreateScoped(scopes);

                // Create Google Calendar API service.
                service = new CalendarService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = credential,
                    ApplicationName = ApplicationName,
                });
            }
        }

        public static void UpdateGoogleCalendar(DateTime start, DateTime end)
        {
            string serviceAccountEmail = "carpooltool@silken-oxygen-161512.iam.gserviceaccount.com";
            string calId = "dc9nerfqo4ob7imj8j2ftdsuok@group.calendar.google.com";

            try
            {
                Init();

                // Define parameters of request.
                EventsResource.ListRequest request = service.Events.List(calId);
                request.TimeMin = start;
                request.TimeMax = end;
                request.ShowDeleted = true;
                request.SingleEvents = true;
                request.MaxResults = 10;
                request.OrderBy = EventsResource.ListRequest.OrderByEnum.StartTime;

                // Remove old events
                Events events = request.Execute();
                if (events.Items != null && events.Items.Count > 0)
                {
                    foreach (var calEvent in events.Items)
                    {
                        if (calEvent.Creator.Email.Equals(serviceAccountEmail))
                        {
                            service.Events.Delete(calId, calEvent.Id);
                        }
                    }
                }

                // Write week events
                var week = EntitiesHelper.GetWeek(start.Date);
                foreach (var day in week)
                {
                    User driver = day.GetByStatus(UserStatus.Driver).FirstOrDefault();
                    List<User> absent = day.GetByStatus(UserStatus.Absent);
                    string summary = null;
                    if(driver == null && absent != null && absent.Count == day.Userdata.Count)
                    {
                        //Tutti per i cazzi propri
                        summary = "CPT: Ognuno per i cazzi propri!";
                    }
                    else
                    {
                        summary = driver != null ? "CPT: guida " + driver.display_name : "CPT: GUIDATORE NON SELEZIONATO";
                        if(absent != null && absent.Count > 0)
                        {
                            summary += " - Assenti: ";
                            for(int i = 0; i < absent.Count; i++)
                            {
                                summary += absent[i].display_name + (i == absent.Count - 1 ? "." : ", ");
                            }
                        }
                    }
                }


            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}