﻿/*                               -*- Mode: C -*- 
 * error.c -- 
 * ITIID           : $ITI$ $Header $__Header$
 * Author          : Thomas Biskup
 * Created On      : Mon Dec 30 00:16:03 1996
 * Last Modified By: Thomas Biskup
 * Last Modified On: Thu Jan  9 18:50:35 1997
 * Update Count    : 2
 * Status          : Unknown, Use with caution!
 *
 * (C) Copyright 1996, 1997 by Thomas Biskup.
 * All Rights Reserved.
 *
 * This software may be distributed only for educational, research and
 * other non-proft purposes provided that this copyright notice is left
 * intact.  If you derive a new game from these sources you also are
 * required to give credit to Thomas Biskup for creating them in the first
 * place.  These sources must not be distributed for any fees in excess of
 * $3 (as of January, 1997).
 */

using System;

namespace HackSharp
{
    internal class Error
    {
        /// <summary>
        /// Die with an error message.
        /// </summary>
        /// <param name="message"></param>
        private void die(string message)
        {
            Console.WriteLine("HackSharp -- Internal error!\n*** Bug: {0}.\n", message);
            Environment.Exit(1);
        }
    }
}