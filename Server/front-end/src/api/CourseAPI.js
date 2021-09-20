import urlconfig from "@/config/urlconfig";
import StorageHelper from "@/helpers/StorageHelper";

async function getCourses() {
    return await fetch(`${urlconfig.protocol}://${urlconfig.base}${urlconfig.course}`, {
        method: "GET",
        headers: new Headers({
            'Authorization': 'Bearer ' + StorageHelper.get("login-token"),
            'Content-Type': 'application/json'
        })
    }).then(async response => {
        return {status: response.status, body: await response.json()};
    });
}

async function getCourse(id) {
    return await fetch(`${urlconfig.protocol}://${urlconfig.base}${urlconfig.course}/${id}`, {
        method: "GET",
        headers: new Headers({
            'Authorization': 'Bearer ' + StorageHelper.get("login-token"),
            'Content-Type': 'application/json'
        })
    }).then(async response => {
        return {status: response.status, body: await response.json()};
    });
}

/**
 *
 * @returns {Promise<number>}
 * @param OwnerUsername
 * @param CourseName
 * @param ShortCourseName
 * @param SDUCourseID
 * @param CourseID
 */
async function putCourse(OwnerUsername, CourseName, ShortCourseName, SDUCourseID, CourseID) {
    return await fetch(`${urlconfig.protocol}://${urlconfig.base}${urlconfig.course}/${CourseID}`, {
        method: "PUT",
        headers: new Headers({
            'Authorization': 'Bearer ' + StorageHelper.get("login-token"),
            'Content-Type': 'application/json'
        }),
        body: JSON.stringify({
            OwnerUsername: OwnerUsername,
            CourseName: CourseName,
            ShortCourseName: ShortCourseName,
            SDUCourseID: SDUCourseID,
            CourseID: CourseID
        })
    }).then(response => response.status);
}

/**
 *
 * @returns {Promise<number>}
 * @param OwnerUsername
 * @param CourseName
 * @param ShortCourseName
 * @param SDUCourseID
 */
async function postCourse(OwnerUsername, CourseName, ShortCourseName, SDUCourseID) {
    return await fetch(`${urlconfig.protocol}://${urlconfig.base}${urlconfig.course}`, {
        method: "POST",
        headers: new Headers({
            'Authorization': 'Bearer ' + StorageHelper.get("login-token"),
            'Content-Type': 'application/json'
        }),
        body: JSON.stringify({
            OwnerUsername: OwnerUsername,
            CourseName: CourseName,
            ShortCourseName: ShortCourseName,
            SDUCourseID: SDUCourseID
        })
    }).then(response => response.status);
}

/**
 *
 * @param id of the course to delete
 * @returns Status code
 */
async function deleteCourse(id) {
    return await fetch(`${urlconfig.protocol}://${urlconfig.base}${urlconfig.course}/${id}`,
        {
            method: 'DELETE',
            headers: new Headers({
                'Authorization': 'Bearer ' + StorageHelper.get("login-token"),
                'Content-Type': 'application/json'
            })
        }).then(response => response.status);
}


export default {
    getCourses,
    getCourse,
    putCourse,
    postCourse,
    deleteCourse
};