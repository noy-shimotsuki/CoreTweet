// The MIT License (MIT)
//
// CoreTweet - A .NET Twitter Library supporting Twitter API 1.1
// Copyright (c) 2013-2018 CoreTweet Development Team
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using CoreTweet.Core;
using Newtonsoft.Json;

namespace CoreTweet.V2
{
    public class RetweetedUsersMeta : CoreBase
    {
        /// <summary>
        /// Pagination token for the previous page of results.
        /// </summary>
        [JsonProperty("previous_token")]
        public string PreviousToken { get; set; }

        /// <summary>
        /// Pagination token for the next page of results.
        /// </summary>
        [JsonProperty("next_token")]
        public string NextToken { get; set; }

        /// <summary>
        /// The number of users returned in this request.
        /// </summary>
        [JsonProperty("result_count")]
        public int ResultCount { get; set; }
    }

    public class RetweetedUsersResponse : UsersResponse
    {
        /// <summary>
        /// Returns the number of users returned in the current request, and pagination details.
        /// </summary>
        [JsonProperty("meta")]
        public RetweetedUsersMeta Meta { get; set; }
    }

    public class RetweetResult : CoreBase
    {
        [JsonProperty("retweeted")]
        public bool Retweeted { get; set; }
    }
}
