// SampleDebugee.cpp : Defines the entry point for the console application.
//

#include "stdafx.h"
#include <string>
#include <iostream>
#include <vector>
#include <map>
#include <unordered_map>
#include <algorithm>
#include <memory>

using namespace std;

template <class T>
inline void hash_combine(size_t& seed, const T& v)
{
	hash<T> hasher;
	seed ^= hasher(v) + 0x9e3779b9 + (seed << 6) + (seed >> 2);
}

class Person
{
	string firstName_;
	string lastName_;

public:
	Person(string firstName, string lastName)
		: firstName_(firstName)
		, lastName_(lastName){}
	
	Person(Person&& other)
		: firstName_(std::move(other.firstName_)),
		lastName_(std::move(other.lastName_))
	{
	}
	
	Person& operator=(Person&& other)
	{
		if (this == &other)
			return *this;
		firstName_ = std::move(other.firstName_);
		lastName_ = std::move(other.lastName_);
		return *this;
	}

	Person(const Person& other) = delete;
	Person& operator=(const Person& other) = delete;

	friend size_t hash_value(const Person& obj)
	{
		size_t seed = 0x25CAED3F;
		hash_combine(seed, obj.firstName_);
		hash_combine(seed, obj.lastName_);
		return seed;
	}

	friend void swap(Person& lhs, Person& rhs)
	{
		using std::swap;
		swap(lhs.firstName_, rhs.firstName_);
		swap(lhs.lastName_, rhs.lastName_);
	}

	friend std::ostream& operator<<(std::ostream& os, const Person& obj)
	{
		return os
			<< obj.firstName_
			<< " " << obj.lastName_;
	}

	friend bool operator==(const Person& lhs, const Person& rhs)
	{
		return lhs.firstName_ == rhs.firstName_
			&& lhs.lastName_ == rhs.lastName_;
	}

	friend bool operator!=(const Person& lhs, const Person& rhs)
	{
		return !(lhs == rhs);
	}

	friend bool lessFirstNameLastName(const Person& lhs, const Person& rhs)
	{
		if (lhs.firstName_ < rhs.firstName_)
			return true;
		if (rhs.firstName_ < lhs.firstName_)
			return false;
		return lhs.lastName_ < rhs.lastName_;
	}

	friend bool lessLastNameFirstName(const Person& lhs, const Person& rhs)
	{
		if (lhs.lastName_ < rhs.lastName_)
			return true;
		if (rhs.lastName_ < lhs.lastName_)
			return false;
		return lhs.firstName_ < rhs.firstName_;
	}
};

class Conference
{
	string name_;
	string venue_;
public:
	Conference(string name, string venue)
		: name_(name)
		, venue_(venue){}


	friend size_t hash_value(const Conference& obj)
	{
		size_t seed = 0x74058D25;
		hash_combine(seed, obj.name_);
		hash_combine(seed, obj.venue_);
		return seed;
	}
};

namespace std
{
	template<>
	struct hash<Person>
	{
		size_t operator()(const Person& _Keyval) const
		{	// hash _Keyval to size_t value by pseudorandomizing transform
			return (hash_value(_Keyval));
		}
	};

	template<>
	struct hash<Conference>
	{
		size_t operator()(const Conference& _Keyval) const
		{	// hash _Keyval to size_t value by pseudorandomizing transform
			return (hash_value(_Keyval));
		}
	};
}

class ConverenceAttendees
{
	unordered_multimap<Conference const*, shared_ptr<Person>> attendees_;
public:	
	auto addAttendee(Conference const&conference, shared_ptr<Person>& attendee)
	{
		attendees_.insert({ &conference,attendee });
		return;
	}
};

auto printPerson(Person const& person, std::ostream& os)
{
	os << person;
	return;
}

long add(int a, int b)
{
	auto res = long(a);
	res += b;
	return res;	
}

void doAddition(ostream& s)
{
	int a = 10;
	int b = 20;

	auto res = add(a, b);
	a =+ 1000;
	res = add(res, a);
	s << res << endl; 
}

int main()
{
	Conference PSConfEU("PSConfEU", "Hannover");

	auto speaker = make_shared<Person>("Staffan", "Gustafsson");
	auto hangAround = make_shared<Person>("Jeffrey", "Snover");
	auto organizer = make_shared<Person>("Tobias", "Weltner");
	printPerson(*speaker, cout);	
	vector<shared_ptr<Person>> people;	
	people.push_back(speaker);
	people.push_back(hangAround);
	people.push_back(organizer);		
		 

	ConverenceAttendees ca;

	for(auto& p : people)
	{
		ca.addAttendee(PSConfEU, p);
	}

	doAddition(cout);

    return 0;
}

