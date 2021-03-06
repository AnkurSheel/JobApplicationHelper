﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

using JAH.DomainModels;
using JAH.Web.Controllers;

using Microsoft.AspNetCore.Mvc;

using Newtonsoft.Json;

using NSubstitute;

using Xunit;

namespace JAH.Web.UnitTests
{
    public class JobApplicationControllerTest : IDisposable
    {
        private readonly JobApplicationsController _jobApplicationsController;

        private readonly FakeHttpMessageHandler _httpMessageHandler;

        private readonly HttpRequestMessage _httpRequestMessage;

        private readonly List<JobApplication> _expectedJobApplications;

        public JobApplicationControllerTest()
        {
            _httpMessageHandler = Substitute.ForPartsOf<FakeHttpMessageHandler>();

            var httpClient = new HttpClient(_httpMessageHandler) { BaseAddress = new Uri("http://localhost/api/") };
            _httpRequestMessage = new HttpRequestMessage();
            _httpMessageHandler.WhenForAnyArgs(x => x.Send(_httpRequestMessage)).DoNotCallBase();

            _jobApplicationsController = new JobApplicationsController(httpClient);

            _expectedJobApplications = new List<JobApplication>
                                       {
                                           new JobApplication
                                           {
                                               CompanyName = "Company 1",
                                               ApplicationDate = new DateTime(2017, 11, 13),
                                               Status = Status.Interview
                                           },
                                           new JobApplication
                                           {
                                               CompanyName = "Company 2",
                                               ApplicationDate = new DateTime(2017, 11, 14),
                                               Status = Status.Applied
                                           },
                                           new JobApplication
                                           {
                                               CompanyName = "Company 3",
                                               ApplicationDate = new DateTime(2017, 11, 14),
                                               Status = Status.Offer
                                           }
                                       };
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        [Fact]
        public async Task ListAllApplications_MultipleApplications_ViewResultWithAllJobApplications()
        {
            // Arrange
            var httpResponseMessage = new HttpResponseMessage
                                      {
                                          StatusCode = HttpStatusCode.OK,
                                          Content = new StringContent(JsonConvert.SerializeObject(_expectedJobApplications))
                                      };

            _httpMessageHandler.Send(_httpRequestMessage).ReturnsForAnyArgs(httpResponseMessage);

            // Act
            IActionResult result = await _jobApplicationsController.ListAllApplications().ConfigureAwait(false);

            // Assert
            Assert.IsType<ViewResult>(result);
            var viewResult = (ViewResult)result;
            Assert.Equal(_expectedJobApplications, viewResult.Model);
        }

        [Fact]
        public async Task ListAllApplications_NoApplications_EmptyViewResult()
        {
            // Arrange
            const string expectedJson = "";
            var httpResponseMessage = new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = new StringContent(expectedJson) };

            var httpRequestMessage = new HttpRequestMessage();
            _httpMessageHandler.WhenForAnyArgs(x => x.Send(httpRequestMessage)).DoNotCallBase();
            _httpMessageHandler.Send(httpRequestMessage).ReturnsForAnyArgs(httpResponseMessage);

            // Act
            IActionResult result = await _jobApplicationsController.ListAllApplications().ConfigureAwait(false);

            // Assert
            Assert.IsType<ViewResult>(result);
            var viewResult = (ViewResult)result;
            Assert.Equal(new List<JobApplication>(), viewResult.Model);
        }

        [Fact]
        public async void AddNewApplication_ApplicationExists_501()
        {
            // Arrange
            var jobApplication = new JobApplication
                                 {
                                     CompanyName = "Company 1",
                                     ApplicationDate = new DateTime(2017, 11, 13),
                                     Status = Status.Interview
                                 };
            var httpResponseMessage = new HttpResponseMessage
                                      {
                                          StatusCode = HttpStatusCode.BadRequest,
                                          Content =
                                              new StringContent(JsonConvert
                                                                    .SerializeObject("{Duplicate Name\": [\"Name Company 1 already exists.\"]}"))
                                      };

            _httpMessageHandler.Send(_httpRequestMessage).ReturnsForAnyArgs(httpResponseMessage);

            // Act
            IActionResult result = await _jobApplicationsController.AddNewApplication(jobApplication).ConfigureAwait(false);

            // Assert
            Assert.IsType<StatusCodeResult>(result);
            var statusCodeResult = (StatusCodeResult)result;
            Assert.Equal(400, statusCodeResult.StatusCode);
        }

        [Fact]
        public async void AddNewApplication_ApplicationDoesNotExists_RedirectToActionObjectResult()
        {
            // Arrange
            var jobApplication = new JobApplication
                                 {
                                     CompanyName = "Company 1",
                                     ApplicationDate = new DateTime(2017, 11, 13),
                                     Status = Status.Interview
                                 };
            var httpResponseMessage = new HttpResponseMessage
                                      {
                                          StatusCode = HttpStatusCode.OK,
                                          Content = new StringContent(JsonConvert.SerializeObject(jobApplication))
                                      };

            _httpMessageHandler.Send(_httpRequestMessage).ReturnsForAnyArgs(httpResponseMessage);

            // Act
            IActionResult result = await _jobApplicationsController.AddNewApplication(jobApplication).ConfigureAwait(false);

            // Assert
            Assert.IsType<RedirectToActionResult>(result);
            var redirectToActionResult = (RedirectToActionResult)result;
            Assert.Equal("ListAllApplications", redirectToActionResult.ActionName);
        }

        [Fact]
        public async Task ShowApplication_ApplicationExists_ViewResultWithJobApplication()
        {
            // Arrange
            const int index = 0;

            var httpResponseMessage = new HttpResponseMessage
                                      {
                                          StatusCode = HttpStatusCode.OK,
                                          Content =
                                              new StringContent(JsonConvert.SerializeObject(_expectedJobApplications[index]))
                                      };

            _httpMessageHandler.Send(_httpRequestMessage).ReturnsForAnyArgs(httpResponseMessage);

            // Act
            IActionResult result =
                await _jobApplicationsController.ShowApplication(_expectedJobApplications[index].CompanyName).ConfigureAwait(false);

            // Assert
            Assert.IsType<ViewResult>(result);
            var viewResult = (ViewResult)result;
            Assert.Equal(_expectedJobApplications[index], viewResult.Model);
        }

        [Fact]
        public async Task ShowApplication_ApplicationDoesNotExist_EmptyViewResult()
        {
            // Arrange
            const string expectedJson = "";
            var httpResponseMessage = new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = new StringContent(expectedJson) };

            var httpRequestMessage = new HttpRequestMessage();
            _httpMessageHandler.WhenForAnyArgs(x => x.Send(httpRequestMessage)).DoNotCallBase();
            _httpMessageHandler.Send(httpRequestMessage).ReturnsForAnyArgs(httpResponseMessage);

            // Act
            IActionResult result = await _jobApplicationsController.ShowApplication("Company").ConfigureAwait(false);

            // Assert
            Assert.IsType<ViewResult>(result);
            var viewResult = (ViewResult)result;
            Assert.Equal(new JobApplication(), viewResult.Model);
        }

        [Fact]
        public async Task UpdateApplication_RedirectToActionObjectResult()
        {
            var jobApplication = new JobApplication
                                 {
                                     CompanyName = "Company 1",
                                     ApplicationDate = new DateTime(2017, 11, 13),
                                     Status = Status.Interview
                                 };
            var httpResponseMessage = new HttpResponseMessage { StatusCode = HttpStatusCode.OK };

            _httpMessageHandler.Send(_httpRequestMessage).ReturnsForAnyArgs(httpResponseMessage);

            // Act
            IActionResult result = await _jobApplicationsController.UpdateApplication(1, jobApplication).ConfigureAwait(false);

            // Assert
            Assert.IsType<RedirectToActionResult>(result);
            var redirectToActionResult = (RedirectToActionResult)result;
            Assert.Equal("ListAllApplications", redirectToActionResult.ActionName);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _jobApplicationsController?.Dispose();
                _httpMessageHandler?.Dispose();
                _httpRequestMessage?.Dispose();
            }
        }
    }
}
