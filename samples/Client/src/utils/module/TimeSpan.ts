export class TimeSpan {
	static fromSeconds(seconds: number): number {
		return seconds * 1000
	}

	static fromMinutes(minutes: number): number {
		return minutes * 1000 * 60
	}

	static fromHours(hours: number): number {
		return hours * 1000 * 60 * 60
	}

	static fromDays(days: number): number {
		return days * 1000 * 60 * 60 * 24
	}
}
