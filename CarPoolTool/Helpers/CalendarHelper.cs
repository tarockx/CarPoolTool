using CarPoolTool.Models;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Hosting;

namespace CarPoolTool.Helpers
{
    public class CalendarHelper
    {
        static private CalendarService service;

        public static DateTime GetMonday(DateTime date, bool skipAheadIfWeekend)
        {
            bool weekend = date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday;

            while (date.DayOfWeek != DayOfWeek.Monday)
            {
                date = weekend && skipAheadIfWeekend ? date.AddDays(1) : date.AddDays(-1);
            }

            return date.Date;
        }

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
                request.ShowDeleted = false;
                request.SingleEvents = true;
                request.MaxResults = 50;
                request.OrderBy = EventsResource.ListRequest.OrderByEnum.StartTime;

                // Remove old events
                Events events = request.Execute();
                if (events.Items != null && events.Items.Count > 0)
                {
                    foreach (var calEvent in events.Items)
                    {
                        if (calEvent.Creator.Email.Equals(serviceAccountEmail))
                        {
                            tryExecuteEvent(service.Events.Delete(calId, calEvent.Id), 3);
                        }
                    }
                }

                // Write week events
                var week = EntitiesHelper.GetWeek(start.Date, end.Date);
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
                        summary = driver != null ? "CPT: " + driver.display_name : "CPT: NESSUN GUIDATORE";
                        if(absent != null && absent.Count > 0)
                        {
                            summary += " - Assenti: ";
                            for(int i = 0; i < absent.Count; i++)
                            {
                                summary += absent[i].display_name + (i == absent.Count - 1 ? "." : ", ");
                            }
                        }
                    }

                    Event body = new Event();
                    body.Summary = summary;
                    body.Start = new EventDateTime() { Date = day.Date.Date.ToString("yyyy-MM-dd") };
                    body.End = new EventDateTime() { Date = day.Date.Date.AddDays(1).ToString("yyyy-MM-dd") };
                    tryExecuteEvent(service.Events.Insert(body, calId), 3);

                    //Alerts
                    foreach (var alert in day.Alerts)
                    {
                        Event alertEvent = new Event();
                        alertEvent.Summary = "CPT ALERT: " + alert.message;
                        if(alert.weekly == 0)
                        {
                            alertEvent.Start = new EventDateTime() { Date = day.Date.Date.ToString("yyyy-MM-dd") };
                            alertEvent.End = new EventDateTime() { Date = day.Date.Date.AddDays(1).ToString("yyyy-MM-dd") };
                        }
                        else
                        {
                            DateTime monday = GetMonday(day.Date, false);
                            alertEvent.Start = new EventDateTime() { Date = monday.Date.ToString("yyyy-MM-dd") };
                            alertEvent.End = new EventDateTime() { Date = monday.Date.AddDays(5).ToString("yyyy-MM-dd") };
                        }
                        tryExecuteEvent(service.Events.Insert(alertEvent, calId), 3);
                    }
                }


            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private static void tryExecuteEvent(EventsResource.DeleteRequest gEvent, int tries)
        {
            for (int i = 0; i < tries; i++)
            {
                try
                {
                    gEvent.Execute();
                    return;
                }
                catch { };
            }
        }

        private static void tryExecuteEvent(EventsResource.InsertRequest gEvent, int tries)
        {
            for (int i = 0; i < tries; i++)
            {
                try
                {
                    gEvent.Execute();
                    return;
                }
                catch { };
            }
        }
    }
}