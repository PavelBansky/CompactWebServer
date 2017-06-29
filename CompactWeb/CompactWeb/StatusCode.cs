//------------------------------------------------------------------------------
// StatusCode.cs
//
// http://bansky.net
//
//------------------------------------------------------------------------------
namespace CompactWeb
{
    /// <summary>
    /// HTTPS StatusCodes
    /// </summary>
    internal enum StatusCode { 
        /// <summary>
        /// 200 OK
        /// </summary>
        OK, 
        /// <summary>
        /// 400 Bad Request
        /// </summary>
        BadRequest, 
        /// <summary>
        /// 404 File not found
        /// </summary>
        NotFound,
        /// <summary>
        /// 403 Access Forbidden
        /// </summary>
        Forbiden };
}
