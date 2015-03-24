﻿﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vizr.API
{
    public interface IResultProvider
    {
        /// <summary>
        /// Unique ID of provider. Use <code>Vizr.API.Hash.CreateFrom("com.example.myprovider")</code>
        /// </summary>
        Hash ID { get; set; }

        /// <summary>
        /// Called when the application is loaded in the background, for example on system start
        /// </summary>
        void OnBackgroundStart();

        /// <summary>
        /// Called when application is brought onto the screen, for example when hotkey is pressed
        /// </summary>
        void OnAppStart();

        /// <summary>
        /// Called when application is closed from the screen
        /// </summary>
        void OnAppHide();

        /// <summary>
        /// Gives the text entered into the application and expects a list of Result items
        /// </summary>
        /// <param name="message">Search query from the user</param>
        /// <returns>List of Result based on input <paramref name="message"/></returns>
        IEnumerable<IResult> Query(string message);
    }
}
