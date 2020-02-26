export const formatDate = (dateParam: Date) => {
    const date = new Date(dateParam);
    return `${date.getDate()}/${date.getMonth() + 1}/${date.getFullYear()} - ${date.getHours()}:${date.getMinutes()}:${date.getSeconds()}`;
}